"""Build the project-owned Haetae General model and Unity FBX outputs.

Run with Blender 4.5 LTS:
    blender --background --python create_haetae_general.py
"""

from pathlib import Path
import math

import bpy
from mathutils import Vector


SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_DIR = SCRIPT_DIR.parent.parent
MODEL_DIR = PROJECT_DIR / "Assets" / "Game" / "Art" / "Models" / "Haetae"
BLEND_PATH = SCRIPT_DIR / "Haetae_General.blend"
LOD0_PATH = MODEL_DIR / "Haetae_General_LOD0.fbx"
LOD1_PATH = MODEL_DIR / "Haetae_General_LOD1.fbx"
PREVIEW_PATH = MODEL_DIR / "Haetae_General_Preview.png"

MODEL_COLLECTION = "HAETAE_GENERAL"
mesh_objects = []
model_objects = []


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials,
                       bpy.data.cameras, bpy.data.lights, bpy.data.armatures):
        for block in list(datablocks):
            if block.users == 0:
                datablocks.remove(block)


def make_material(name, color, metallic, roughness, emission=None, strength=0.0):
    material = bpy.data.materials.new(name)
    material.diffuse_color = (*color, 1.0)
    material.use_nodes = True
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color, 1.0)
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Roughness"].default_value = roughness
    if emission is not None:
        emission_input = shader.inputs.get("Emission Color") or shader.inputs.get("Emission")
        strength_input = shader.inputs.get("Emission Strength")
        if emission_input is not None:
            emission_input.default_value = (*emission, 1.0)
        if strength_input is not None:
            strength_input.default_value = strength
    return material


def finish_mesh(obj, material, bevel=0.04, smooth=True, bone="body"):
    obj.data.materials.append(material)
    if bevel > 0.0:
        modifier = obj.modifiers.new("Production Bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 3
        modifier.limit_method = "ANGLE"
        modifier.angle_limit = math.radians(28.0)
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.select_set(False)
    if smooth:
        for polygon in obj.data.polygons:
            polygon.use_smooth = True
    obj["rig_bone"] = bone
    mesh_objects.append(obj)
    model_objects.append(obj)
    return obj


def box(name, location, scale, material, rotation=(0.0, 0.0, 0.0),
        bevel=0.06, bone="body"):
    bpy.ops.mesh.primitive_cube_add(location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish_mesh(obj, material, bevel, False, bone)


def wedge(name, location, length, rear_width, front_width, rear_height,
          front_height, material, rotation=(0.0, 0.0, 0.0), bevel=0.05,
          bone="body"):
    half_length = length * 0.5
    vertices = [
        (-rear_width, half_length, -rear_height),
        (rear_width, half_length, -rear_height),
        (rear_width, half_length, rear_height),
        (-rear_width, half_length, rear_height),
        (-front_width, -half_length, -front_height),
        (front_width, -half_length, -front_height),
        (front_width, -half_length, front_height),
        (-front_width, -half_length, front_height),
    ]
    faces = [
        (0, 1, 2, 3), (4, 7, 6, 5),
        (0, 4, 5, 1), (3, 2, 6, 7),
        (0, 3, 7, 4), (1, 5, 6, 2),
    ]
    mesh = bpy.data.meshes.new(name + "_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.location = location
    obj.rotation_euler = rotation
    return finish_mesh(obj, material, bevel, False, bone)


def profile_plate(name, points, thickness, location, material,
                  rotation=(0.0, 0.0, 0.0), bevel=0.035, bone="body"):
    """Extrude an authored 2D silhouette instead of exposing a box primitive."""
    half = thickness * 0.5
    count = len(points)
    vertices = [(x, y, -half) for x, y in points]
    vertices.extend((x, y, half) for x, y in points)
    faces = [tuple(range(count - 1, -1, -1)), tuple(range(count, count * 2))]
    for index in range(count):
        next_index = (index + 1) % count
        faces.append((
            index, next_index, count + next_index, count + index))
    mesh = bpy.data.meshes.new(name + "_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.location = location
    obj.rotation_euler = rotation
    return finish_mesh(obj, material, bevel, False, bone)


def sphere(name, location, scale, material, segments=24, rings=12,
           bone="body"):
    bpy.ops.mesh.primitive_uv_sphere_add(
        segments=segments, ring_count=rings, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish_mesh(obj, material, 0.0, True, bone)


def cylinder_between(name, start, end, radius, material, vertices=16,
                     bevel=0.025, bone="body", taper=1.0):
    start_vector = Vector(start)
    end_vector = Vector(end)
    delta = end_vector - start_vector
    midpoint = (start_vector + end_vector) * 0.5
    if abs(taper - 1.0) < 0.001:
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=vertices, radius=radius, depth=delta.length,
            location=midpoint)
    else:
        bpy.ops.mesh.primitive_cone_add(
            vertices=vertices, radius1=radius, radius2=radius * taper,
            depth=delta.length, location=midpoint)
    obj = bpy.context.object
    obj.name = name
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = delta.to_track_quat("Z", "Y")
    obj.rotation_mode = "XYZ"
    return finish_mesh(obj, material, bevel, True, bone)


def torus(name, location, major_radius, minor_radius, material,
          rotation=(0.0, 0.0, 0.0), scale=(1.0, 1.0, 1.0), bone="body"):
    bpy.ops.mesh.primitive_torus_add(
        major_radius=major_radius, minor_radius=minor_radius,
        major_segments=24, minor_segments=8,
        location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish_mesh(obj, material, 0.0, True, bone)


def curve_tube(name, points, radius, material, bone="body"):
    curve = bpy.data.curves.new(name + "_Curve", "CURVE")
    curve.dimensions = "3D"
    curve.resolution_u = 3
    curve.bevel_depth = radius
    curve.bevel_resolution = 3
    spline = curve.splines.new("BEZIER")
    spline.bezier_points.add(len(points) - 1)
    for point, coordinate in zip(spline.bezier_points, points):
        point.co = coordinate
        point.handle_left_type = "AUTO"
        point.handle_right_type = "AUTO"
    obj = bpy.data.objects.new(name, curve)
    bpy.context.collection.objects.link(obj)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.convert(target="MESH")
    obj = bpy.context.object
    return finish_mesh(obj, material, 0.0, True, bone)


def spiral_tube(name, side_x, center_y, center_z, outer_radius,
                material, turns=1.55, tube_radius=0.026, bone="body"):
    points = []
    samples = 34
    for index in range(samples):
        progress = index / (samples - 1)
        angle = math.radians(-35.0) + progress * turns * math.tau
        radius = outer_radius * (1.0 - progress * 0.72)
        points.append((
            side_x,
            center_y + math.cos(angle) * radius * 0.84,
            center_z + math.sin(angle) * radius,
        ))
    return curve_tube(name, points, tube_radius, material, bone)


def build_materials():
    return {
        "navy": make_material("MAT_NavyFrame", (0.018, 0.035, 0.075), 0.82, 0.24),
        "ivory": make_material("MAT_IvoryArmor", (0.72, 0.67, 0.54), 0.52, 0.28),
        "gold": make_material("MAT_GoldTrim", (0.53, 0.29, 0.07), 0.9, 0.2),
        "cyan": make_material(
            "MAT_CyanEnergy", (0.01, 0.32, 0.42), 0.25, 0.16,
            emission=(0.0, 0.85, 1.0), strength=5.5),
        "joint": make_material("MAT_DarkJoint", (0.012, 0.015, 0.022), 0.72, 0.32),
    }


def build_body(materials):
    navy = materials["navy"]
    ivory = materials["ivory"]
    gold = materials["gold"]
    cyan = materials["cyan"]
    joint = materials["joint"]

    sphere("Frame_Core", (0.0, 0.05, 1.38), (0.86, 1.28, 0.62), navy,
           segments=32, rings=16)
    wedge("Back_Armor_Main", (0.0, 0.18, 1.83), 1.55, 0.68, 0.52,
          0.14, 0.1, ivory, bevel=0.08)
    profile_plate(
        "Back_Crown_Profile",
        [(-0.52, -0.78), (0.52, -0.78), (0.67, -0.5),
         (0.61, 0.57), (0.35, 0.79), (-0.35, 0.79), (-0.61, 0.57),
         (-0.67, -0.5)],
        0.12, (0.0, 0.18, 2.025), ivory, bevel=0.055)
    profile_plate(
        "Back_Gold_Inlay",
        [(-0.34, -0.61), (0.34, -0.61), (0.43, -0.43),
         (0.38, 0.47), (0.22, 0.61), (-0.22, 0.61), (-0.38, 0.47),
         (-0.43, -0.43)],
        0.035, (0.0, 0.18, 2.1), gold, bevel=0.018)
    wedge("Chest_Armor", (0.0, -0.93, 1.38), 0.78, 0.58, 0.42,
          0.42, 0.34, ivory, rotation=(math.radians(13), 0.0, 0.0),
          bevel=0.075)
    box("Spine_Rail_Left", (-0.34, 0.25, 1.97), (0.07, 0.72, 0.055),
        gold, bevel=0.025)
    box("Spine_Rail_Right", (0.34, 0.25, 1.97), (0.07, 0.72, 0.055),
        gold, bevel=0.025)
    box("Spine_Energy", (0.0, 0.25, 1.99), (0.06, 0.68, 0.035),
        cyan, bevel=0.018)

    for side in (-1, 1):
        x = side * 0.88
        sphere("Shoulder_Frame_L" if side < 0 else "Shoulder_Frame_R",
               (x, -0.52, 1.53), (0.34, 0.46, 0.46), joint,
               segments=24, rings=12)
        torus("Shoulder_Gold_Ring_L" if side < 0 else "Shoulder_Gold_Ring_R",
              (side * 1.145, -0.5, 1.56), 0.27, 0.055, gold,
              rotation=(0.0, math.radians(90), 0.0),
              scale=(1.0, 1.0, 1.16))
        wedge("Shoulder_Armor_L" if side < 0 else "Shoulder_Armor_R",
              (side * 0.96, -0.54, 1.66), 0.76, 0.31, 0.24,
              0.35, 0.28, ivory,
              rotation=(math.radians(-8), 0.0, side * math.radians(7)),
              bevel=0.07)
        profile_plate(
            "Shoulder_Crest_L" if side < 0 else "Shoulder_Crest_R",
            [(-0.32, -0.34), (0.12, -0.4), (0.34, -0.18),
             (0.3, 0.18), (0.04, 0.38), (-0.29, 0.27),
             (-0.38, -0.03)],
            0.09, (side * 1.22, -0.55, 1.7), ivory,
            rotation=(0.0, side * math.radians(90), 0.0),
            bevel=0.04)
        spiral_tube(
            "Shoulder_Haetae_Spiral_L"
            if side < 0 else "Shoulder_Haetae_Spiral_R",
            side * 1.275, -0.56, 1.7, 0.22, gold,
            turns=1.62, tube_radius=0.024)
        torus("Reactor_Ring_L" if side < 0 else "Reactor_Ring_R",
              (side * 0.875, 0.12, 1.43), 0.255, 0.052, gold,
              rotation=(0.0, math.radians(90), 0.0),
              scale=(1.0, 1.0, 1.0))
        sphere("Reactor_Core_L" if side < 0 else "Reactor_Core_R",
               (side * 0.92, 0.12, 1.43), (0.095, 0.18, 0.18), cyan,
               segments=20, rings=10)
        profile_plate(
            "Flank_Armor_Profile_L" if side < 0 else "Flank_Armor_Profile_R",
            [(-0.47, -0.6), (-0.25, -0.78), (0.28, -0.7),
             (0.48, -0.42), (0.42, 0.48), (0.18, 0.72),
             (-0.28, 0.65), (-0.46, 0.34)],
            0.11, (side * 1.03, 0.22, 1.48), ivory,
            rotation=(0.0, side * math.radians(90), 0.0),
            bevel=0.05)
        spiral_tube(
            "Haetae_Flank_Spiral_L" if side < 0 else "Haetae_Flank_Spiral_R",
            side * 1.105, 0.11, 1.55, 0.29, gold,
            turns=1.7, tube_radius=0.027)
        curve_tube(
            "Flank_Energy_Channel_L" if side < 0 else "Flank_Energy_Channel_R",
            [
                (side * 1.115, -0.42, 1.28),
                (side * 1.125, -0.08, 1.17),
                (side * 1.13, 0.32, 1.2),
                (side * 1.1, 0.62, 1.38),
            ],
            0.022, cyan)

    for index, y in enumerate((0.72, 0.32, -0.08)):
        wedge("Back_Scale_%02d" % index, (0.0, y, 2.03 + index * 0.025),
              0.34, 0.42 - index * 0.035, 0.33 - index * 0.035,
              0.075, 0.055, ivory, bevel=0.035)
    for index, y in enumerate((-0.45, -0.12, 0.22, 0.56)):
        profile_plate(
            "Underbody_Rib_%02d" % index,
            [(-0.48, -0.12), (0.48, -0.12), (0.38, 0.12),
             (0.0, 0.18), (-0.38, 0.12)],
            0.08, (0.0, y, 0.98), navy,
            rotation=(math.radians(90), 0.0, 0.0), bevel=0.03)


def build_head(materials):
    navy = materials["navy"]
    ivory = materials["ivory"]
    gold = materials["gold"]
    cyan = materials["cyan"]
    joint = materials["joint"]

    sphere("Neck_Joint", (0.0, -1.08, 1.55), (0.46, 0.39, 0.42), joint,
           segments=24, rings=12, bone="head")
    wedge("Head_Frame", (0.0, -1.47, 1.64), 0.82, 0.48, 0.38,
          0.42, 0.3, navy, rotation=(math.radians(-4), 0.0, 0.0),
          bevel=0.075, bone="head")
    wedge("Brow_Armor", (0.0, -1.73, 1.87), 0.46, 0.43, 0.31,
          0.18, 0.13, ivory, rotation=(math.radians(-8), 0.0, 0.0),
          bevel=0.055, bone="head")
    wedge("Muzzle_Armor", (0.0, -1.93, 1.57), 0.5, 0.32, 0.24,
          0.22, 0.16, ivory, rotation=(math.radians(4), 0.0, 0.0),
          bevel=0.05, bone="head")
    wedge("Jaw_Frame", (0.0, -1.88, 1.38), 0.43, 0.27, 0.2,
          0.12, 0.09, navy, rotation=(math.radians(-6), 0.0, 0.0),
          bevel=0.035, bone="head")
    box("Energy_Jaw", (0.0, -2.075, 1.43), (0.23, 0.035, 0.045),
        cyan, bevel=0.018, bone="head")
    profile_plate(
        "Face_Mask_Center",
        [(-0.24, -0.2), (0.24, -0.2), (0.34, 0.01),
         (0.25, 0.27), (0.0, 0.41), (-0.25, 0.27), (-0.34, 0.01)],
        0.075, (0.0, -2.08, 1.69), ivory,
        rotation=(math.radians(90), 0.0, 0.0),
        bevel=0.034, bone="head")
    profile_plate(
        "Face_Mask_Gold_Inlay",
        [(-0.15, -0.14), (0.15, -0.14), (0.21, 0.0),
         (0.14, 0.2), (0.0, 0.3), (-0.14, 0.2), (-0.21, 0.0)],
        0.026, (0.0, -2.127, 1.69), gold,
        rotation=(math.radians(90), 0.0, 0.0),
        bevel=0.014, bone="head")
    curve_tube(
        "Brow_Gold_Sweep",
        [(-0.39, -2.145, 1.88), (-0.22, -2.18, 1.96),
         (0.0, -2.195, 2.02), (0.22, -2.18, 1.96),
         (0.39, -2.145, 1.88)],
        0.024, gold, bone="head")
    curve_tube(
        "Jaw_Energy_Arc",
        [(-0.25, -2.16, 1.48), (0.0, -2.19, 1.42),
         (0.25, -2.16, 1.48)],
        0.018, cyan, bone="head")

    for side in (-1, 1):
        eye_name = "Eye_L" if side < 0 else "Eye_R"
        sphere(eye_name, (side * 0.245, -1.91, 1.78),
               (0.105, 0.035, 0.06), cyan, segments=20, rings=10,
               bone="head")
        box("Cheek_Armor_L" if side < 0 else "Cheek_Armor_R",
            (side * 0.39, -1.69, 1.52), (0.12, 0.28, 0.22), ivory,
            rotation=(math.radians(4), side * math.radians(9),
                      side * math.radians(8)),
            bevel=0.045, bone="head")
        torus("Mane_Curl_L" if side < 0 else "Mane_Curl_R",
              (side * 0.52, -1.35, 1.8), 0.16, 0.045, gold,
              rotation=(0.0, math.radians(90), 0.0),
              scale=(1.0, 1.0, 1.15), bone="head")
        spiral_tube(
            "Mane_Spiral_L" if side < 0 else "Mane_Spiral_R",
            side * 0.57, -1.34, 1.8, 0.23, gold,
            turns=1.55, tube_radius=0.024, bone="head")
        for layer, (center_y, center_z, size) in enumerate((
                (-1.16, 1.91, 0.24),
                (-1.03, 1.69, 0.21),
                (-1.0, 1.47, 0.18))):
            profile_plate(
                ("Mane_Petal_L_%02d" if side < 0
                 else "Mane_Petal_R_%02d") % layer,
                [(-size * 0.62, -size * 0.68),
                 (size * 0.1, -size),
                 (size * 0.58, -size * 0.58),
                 (size * 0.72, size * 0.28),
                 (0.0, size),
                 (-size * 0.72, size * 0.34)],
                0.07, (side * (0.55 + layer * 0.025), center_y, center_z),
                ivory if layer != 1 else navy,
                rotation=(0.0, side * math.radians(90), 0.0),
                bevel=0.028, bone="head")
        sphere("Ear_Base_L" if side < 0 else "Ear_Base_R",
               (side * 0.43, -1.26, 2.03), (0.16, 0.12, 0.13), gold,
               segments=18, rings=9, bone="head")
        wedge("Ear_Fin_L" if side < 0 else "Ear_Fin_R",
              (side * 0.49, -1.24, 2.18), 0.3, 0.12, 0.035,
              0.1, 0.035, ivory,
              rotation=(math.radians(-7), side * math.radians(12),
                        side * math.radians(18)),
              bevel=0.025, bone="head")
        cylinder_between(
            "Fang_L" if side < 0 else "Fang_R",
            (side * 0.19, -2.12, 1.43),
            (side * 0.21, -2.18, 1.24),
            0.05, ivory, vertices=18, bevel=0.012, bone="head",
            taper=0.08)
        cylinder_between(
            "Jaw_Tusk_L" if side < 0 else "Jaw_Tusk_R",
            (side * 0.34, -1.98, 1.48),
            (side * 0.42, -2.06, 1.35),
            0.04, gold, vertices=16, bevel=0.01, bone="head",
            taper=0.1)

    horn_points = [
        (0.0, -1.54, 2.03),
        (0.0, -1.5, 2.28),
        (0.02, -1.39, 2.52),
        (0.05, -1.22, 2.72),
    ]
    for index in range(len(horn_points) - 1):
        cylinder_between(
            "Crown_Horn_%02d" % index, horn_points[index],
            horn_points[index + 1], 0.11 - index * 0.025, gold,
            vertices=20, bevel=0.018, bone="head", taper=0.68)
    box("Forehead_Energy", (0.0, -1.78, 1.97), (0.07, 0.035, 0.11),
        cyan, rotation=(math.radians(-8), 0.0, 0.0),
        bevel=0.018, bone="head")
    torus("Horn_Base_Crown", (0.0, -1.51, 2.08), 0.18, 0.034, gold,
          rotation=(math.radians(90), 0.0, 0.0),
          scale=(1.0, 1.0, 0.72), bone="head")


def build_leg(materials, side, front):
    navy = materials["navy"]
    ivory = materials["ivory"]
    gold = materials["gold"]
    cyan = materials["cyan"]
    joint = materials["joint"]
    side_name = "L" if side < 0 else "R"
    position_name = "F" if front else "B"
    prefix = "Leg_%s%s" % (side_name, position_name)
    bone = prefix.lower()
    x = side * 0.72
    y = -0.72 if front else 0.72
    hip = (x, y, 1.38)
    knee = (side * 0.82, y - (0.12 if front else -0.08), 0.83)
    ankle = (side * 0.74, y - (0.23 if front else -0.18), 0.31)
    paw = (side * 0.74, y - (0.38 if front else -0.24), 0.14)

    sphere(prefix + "_Hip", hip, (0.25, 0.25, 0.25), joint,
           segments=20, rings=10, bone=bone)
    cylinder_between(prefix + "_Upper", hip, knee, 0.18, navy,
                     vertices=16, bevel=0.035, bone=bone, taper=0.78)
    wedge(prefix + "_UpperArmor",
          (side * 0.785, y - (0.06 if front else -0.03), 1.08),
          0.47, 0.19, 0.145, 0.16, 0.12, ivory,
          rotation=(math.radians(-7 if front else 5), 0.0,
                    side * math.radians(3)),
          bevel=0.045, bone=bone)
    sphere(prefix + "_Knee", knee, (0.2, 0.2, 0.2), gold,
           segments=20, rings=10, bone=bone)
    cylinder_between(prefix + "_Lower", knee, ankle, 0.14, joint,
                     vertices=16, bevel=0.028, bone=bone, taper=0.72)
    piston_offset = side * 0.115
    cylinder_between(
        prefix + "_Piston_Rod",
        (hip[0] + piston_offset, hip[1] - 0.03, hip[2] - 0.08),
        (ankle[0] + piston_offset * 0.55, ankle[1], ankle[2] + 0.1),
        0.038, gold, vertices=14, bevel=0.012, bone=bone, taper=0.8)
    cylinder_between(
        prefix + "_Piston_Sleeve",
        (hip[0] + piston_offset, hip[1] - 0.03, hip[2] - 0.08),
        (knee[0] + piston_offset * 0.75, knee[1], knee[2] + 0.08),
        0.065, navy, vertices=16, bevel=0.016, bone=bone, taper=0.72)
    wedge(prefix + "_ShinArmor",
          (side * 0.78, y - (0.18 if front else -0.13), 0.57),
          0.4, 0.155, 0.12, 0.17, 0.13, ivory,
          rotation=(math.radians(-5 if front else 4), 0.0, 0.0),
          bevel=0.04, bone=bone)
    profile_plate(
        prefix + "_KneeGuard",
        [(-0.18, -0.18), (0.1, -0.22), (0.23, -0.04),
         (0.18, 0.16), (0.0, 0.26), (-0.19, 0.14)],
        0.075, (knee[0] + side * 0.18, knee[1], knee[2]), ivory,
        rotation=(0.0, side * math.radians(90), 0.0),
        bevel=0.03, bone=bone)
    curve_tube(
        prefix + "_ShinEnergy",
        [(side * 0.785, knee[1] - 0.02, knee[2] - 0.08),
         (side * 0.79, (knee[1] + ankle[1]) * 0.5, 0.57),
         (side * 0.76, ankle[1] - 0.01, ankle[2] + 0.08)],
        0.016, cyan, bone=bone)
    sphere(prefix + "_Ankle", ankle, (0.16, 0.16, 0.16), joint,
           segments=18, rings=9, bone=bone)
    wedge(prefix + "_Paw", paw, 0.45, 0.22, 0.18, 0.11, 0.08,
          navy, rotation=(math.radians(2), 0.0, 0.0),
          bevel=0.055, bone=bone)
    profile_plate(
        prefix + "_PawCrown",
        [(-0.19, -0.2), (0.19, -0.2), (0.22, 0.02),
         (0.12, 0.2), (-0.12, 0.2), (-0.22, 0.02)],
        0.055, (paw[0], paw[1], paw[2] + 0.105), ivory,
        bevel=0.026, bone=bone)
    for toe in (-1, 0, 1):
        claw_y = -1.0 if front else 1.0
        cylinder_between(
            prefix + "_Claw_%d" % (toe + 2),
            (paw[0] + toe * 0.115, paw[1] + claw_y * 0.12, 0.105),
            (paw[0] + toe * 0.13, paw[1] + claw_y * 0.37, 0.055),
            0.052, gold, vertices=16, bevel=0.012, bone=bone,
            taper=0.05)


def build_tail(materials):
    navy = materials["navy"]
    ivory = materials["ivory"]
    gold = materials["gold"]
    cyan = materials["cyan"]
    joint = materials["joint"]
    points = [
        (0.0, 1.12, 1.47),
        (0.0, 1.48, 1.43),
        (0.08, 1.8, 1.35),
        (0.18, 2.08, 1.2),
        (0.3, 2.3, 0.99),
        (0.38, 2.48, 0.76),
        (0.42, 2.64, 0.6),
    ]
    for index in range(len(points) - 1):
        bone = "tail_%02d" % (index + 1)
        sphere("Tail_Joint_%02d" % index, points[index],
               (0.15 - index * 0.012,) * 3, joint,
               segments=16, rings=8, bone=bone)
        cylinder_between("Tail_Segment_%02d" % index, points[index],
                         points[index + 1], 0.145 - index * 0.013,
                         ivory if index % 2 == 0 else navy,
                         vertices=16, bevel=0.028, bone=bone, taper=0.82)
        midpoint = (Vector(points[index]) + Vector(points[index + 1])) * 0.5
        profile_plate(
            "Tail_Dorsal_Scale_%02d" % index,
            [(-0.11, -0.16), (0.11, -0.16), (0.16, 0.02),
             (0.0, 0.23), (-0.16, 0.02)],
            0.045, (midpoint.x, midpoint.y, midpoint.z + 0.14),
            gold if index % 2 == 0 else ivory,
            rotation=(0.0, 0.0, math.radians(index * 2.0)),
            bevel=0.02, bone=bone)
    curve_tube(
        "Tail_Energy_Spine",
        [(point[0], point[1], point[2] + 0.12) for point in points],
        0.02, cyan, bone="tail_01")
    profile_plate(
        "Tail_Fin_Profile",
        [(-0.2, -0.14), (0.03, -0.24), (0.21, -0.08),
         (0.13, 0.09), (0.0, 0.25), (-0.15, 0.1)],
        0.06, (0.42, 2.72, 0.51), gold,
        rotation=(math.radians(-48), 0.0, math.radians(-8)),
        bevel=0.024, bone="tail_06")


def build_unit_markers(materials):
    cyan = materials["cyan"]
    gold = materials["gold"]
    first = torus("UnitMarker_1", (-0.34, -0.26, 2.065), 0.085, 0.022,
                  cyan, rotation=(0.0, 0.0, 0.0),
                  scale=(1.0, 1.3, 1.0))
    second = torus("UnitMarker_2", (0.34, -0.26, 2.065), 0.085, 0.022,
                   gold, rotation=(0.0, 0.0, 0.0),
                   scale=(1.0, 1.3, 1.0))
    first["unit_marker_index"] = 1
    second["unit_marker_index"] = 2


def build_armature():
    armature_data = bpy.data.armatures.new("Haetae_General_Rig")
    armature = bpy.data.objects.new("Haetae_General_Rig", armature_data)
    bpy.context.collection.objects.link(armature)
    model_objects.append(armature)
    bpy.context.view_layer.objects.active = armature
    armature.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")

    def bone(name, head, tail, parent=None):
        created = armature.data.edit_bones.new(name)
        created.head = head
        created.tail = tail
        if parent is not None:
            created.parent = armature.data.edit_bones.get(parent)
        return created

    bone("root", (0.0, 0.0, 0.0), (0.0, 0.0, 0.4))
    bone("body", (0.0, 0.0, 0.4), (0.0, 0.0, 1.55), "root")
    bone("head", (0.0, -0.9, 1.5), (0.0, -1.75, 1.65), "body")
    for side, side_name in ((-1, "l"), (1, "r")):
        for front, position_name, y in ((True, "f", -0.72), (False, "b", 0.72)):
            bone("leg_%s%s" % (side_name, position_name),
                 (side * 0.72, y, 1.38),
                 (side * 0.76, y, 0.28), "body")
    parent = "body"
    tail_points = [
        (0.0, 1.12, 1.47), (0.0, 1.48, 1.43), (0.08, 1.8, 1.35),
        (0.18, 2.08, 1.2), (0.3, 2.3, 0.99), (0.38, 2.48, 0.76),
        (0.42, 2.64, 0.6),
    ]
    for index in range(6):
        name = "tail_%02d" % (index + 1)
        bone(name, tail_points[index], tail_points[index + 1], parent)
        parent = name

    bpy.ops.object.mode_set(mode="OBJECT")
    armature["asset_id"] = "character.haetae.general"
    armature["source_recipe"] = "ArtSource/Haetae/create_haetae_general.py"
    armature["forward_axis"] = "-Y"
    armature["detail_revision"] = 2
    armature["art_direction"] = (
        "Korean guardian-lion mecha with authored armor profiles, mane spirals, "
        "visible actuators, claws, and tail scales")
    armature.show_in_front = True

    armature.select_set(False)
    return armature


def consolidate_meshes(armature):
    marker_objects = [
        obj for obj in mesh_objects if obj.get("unit_marker_index") is not None]
    body_parts = [obj for obj in mesh_objects if obj not in marker_objects]
    if not body_parts:
        raise RuntimeError("Haetae body contains no mesh parts.")

    for obj in body_parts:
        group = obj.vertex_groups.new(name=obj.get("rig_bone", "body"))
        group.add(range(len(obj.data.vertices)), 1.0, "REPLACE")

    bpy.ops.object.select_all(action="DESELECT")
    for obj in body_parts:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = body_parts[0]
    bpy.ops.object.join()
    body = bpy.context.object
    body.name = "Haetae_General_Body"
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    old_materials = list(body.data.materials)
    unique_materials = []
    unique_indices = {}
    old_to_new = {}
    for old_index, material in enumerate(old_materials):
        key = material.name if material is not None else ""
        if key not in unique_indices:
            unique_indices[key] = len(unique_materials)
            unique_materials.append(material)
        old_to_new[old_index] = unique_indices[key]
    remapped_material_indices = [
        old_to_new.get(polygon.material_index, 0)
        for polygon in body.data.polygons
    ]
    body.data.materials.clear()
    for material in unique_materials:
        body.data.materials.append(material)
    for polygon, material_index in zip(
            body.data.polygons, remapped_material_indices):
        polygon.material_index = material_index

    body.parent = armature
    body.matrix_parent_inverse = armature.matrix_world.inverted()
    modifier = body.modifiers.new("Haetae Rigid Rig", "ARMATURE")
    modifier.object = armature

    for marker in marker_objects:
        world = marker.matrix_world.copy()
        marker.parent = armature
        marker.parent_type = "BONE"
        marker.parent_bone = "body"
        marker.matrix_world = world

    mesh_objects[:] = [body] + marker_objects
    model_objects[:] = [body] + marker_objects + [armature]
    return body


def export_fbx(path, armature, decimate_ratio=None):
    decimate_modifiers = []
    if decimate_ratio is not None:
        for obj in mesh_objects:
            if len(obj.data.vertices) < 80:
                continue
            modifier = obj.modifiers.new("LOD1 Decimate", "DECIMATE")
            modifier.ratio = decimate_ratio
            modifier.use_collapse_triangulate = True
            decimate_modifiers.append((obj, modifier))

    bpy.ops.object.select_all(action="DESELECT")
    for obj in model_objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=True,
        object_types={"ARMATURE", "MESH"},
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
    )
    for obj, modifier in decimate_modifiers:
        obj.modifiers.remove(modifier)


def aim_at(obj, target):
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def render_preview():
    bpy.ops.object.select_all(action="DESELECT")
    bpy.ops.mesh.primitive_plane_add(size=20, location=(0.0, 0.2, 0.0))
    floor = bpy.context.object
    floor.name = "Preview Floor"
    floor_material = make_material(
        "MAT_PreviewFloor", (0.055, 0.06, 0.07), 0.05, 0.72)
    floor.data.materials.append(floor_material)

    bpy.ops.object.camera_add(location=(5.8, -7.8, 3.9))
    camera = bpy.context.object
    camera.name = "Preview Camera"
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 4.75
    aim_at(camera, (0.0, 0.0, 1.35))
    bpy.context.scene.camera = camera

    bpy.ops.object.light_add(type="AREA", location=(3.8, -4.5, 6.3))
    key = bpy.context.object
    key.name = "Preview Key"
    key.data.energy = 1250
    key.data.shape = "DISK"
    key.data.size = 4.0
    aim_at(key, (0.0, 0.0, 1.2))

    bpy.ops.object.light_add(type="AREA", location=(-4.0, -1.5, 3.0))
    fill = bpy.context.object
    fill.name = "Preview Fill"
    fill.data.energy = 800
    fill.data.size = 3.5
    aim_at(fill, (0.0, -0.2, 1.3))

    bpy.ops.object.light_add(type="AREA", location=(0.0, 4.0, 4.0))
    rim = bpy.context.object
    rim.name = "Preview Rim"
    rim.data.energy = 1050
    rim.data.size = 3.0
    aim_at(rim, (0.0, 0.3, 1.45))

    world = bpy.context.scene.world
    world.color = (0.012, 0.016, 0.025)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.render.resolution_x = 1280
    scene.render.resolution_y = 1280
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = str(PREVIEW_PATH)
    scene.render.film_transparent = False
    scene.render.image_settings.color_mode = "RGBA"
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.view_settings.exposure = 0.65
    scene.render.resolution_percentage = 100
    bpy.ops.render.render(write_still=True)


def main():
    MODEL_DIR.mkdir(parents=True, exist_ok=True)
    SCRIPT_DIR.mkdir(parents=True, exist_ok=True)
    reset_scene()
    collection = bpy.data.collections.new(MODEL_COLLECTION)
    bpy.context.scene.collection.children.link(collection)
    bpy.context.view_layer.active_layer_collection = (
        bpy.context.view_layer.layer_collection.children[MODEL_COLLECTION])

    materials = build_materials()
    build_body(materials)
    build_head(materials)
    for side in (-1, 1):
        build_leg(materials, side, True)
        build_leg(materials, side, False)
    build_tail(materials)
    build_unit_markers(materials)
    armature = build_armature()
    consolidate_meshes(armature)

    export_fbx(LOD0_PATH, armature)
    export_fbx(LOD1_PATH, armature, decimate_ratio=0.52)
    render_preview()
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH), compress=True)

    total_vertices = sum(len(obj.data.vertices) for obj in mesh_objects)
    print("HAETAE_BUILD_COMPLETE")
    print("LOD0=" + str(LOD0_PATH))
    print("LOD1=" + str(LOD1_PATH))
    print("PREVIEW=" + str(PREVIEW_PATH))
    print("BLEND=" + str(BLEND_PATH))
    print("MESH_OBJECTS=" + str(len(mesh_objects)))
    print("SOURCE_VERTICES=" + str(total_vertices))
    print("DETAIL_REVISION=2")


if __name__ == "__main__":
    main()
