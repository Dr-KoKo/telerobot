"""Generate authored Melee, Ranged and Balanced Haetae upgrade models.

Run with Blender 4.5 LTS:
    blender --background --factory-startup --python create_haetae_upgrades.py
"""

from pathlib import Path
import math
import sys

import bpy
from mathutils import Vector


SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_DIR = SCRIPT_DIR.parent.parent
MODEL_DIR = PROJECT_DIR / "Assets" / "Game" / "Art" / "Models" / "Haetae"
GALLERY_PATH = MODEL_DIR / "Haetae_Upgrades_Gallery.png"

if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

import create_haetae_general as general


VARIANTS = (
    {
        "role": "Melee",
        "asset_id": "character.haetae.melee",
        "signature": "haetae.authored.melee.ram",
    },
    {
        "role": "Ranged",
        "asset_id": "character.haetae.ranged",
        "signature": "haetae.authored.ranged.turret",
    },
    {
        "role": "Balanced",
        "asset_id": "character.haetae.balanced",
        "signature": "haetae.authored.balanced.asymmetric",
    },
)


def role_paths(role):
    stem = "Haetae_" + role
    return {
        "blend": SCRIPT_DIR / (stem + ".blend"),
        "lod0": MODEL_DIR / (stem + "_LOD0.fbx"),
        "lod1": MODEL_DIR / (stem + "_LOD1.fbx"),
        "preview": MODEL_DIR / (stem + "_Preview.png"),
    }


def begin_variant():
    general.reset_scene()
    general.mesh_objects.clear()
    general.model_objects.clear()


def build_common_body(materials):
    general.build_body(materials)
    general.build_head(materials)
    for side in (-1, 1):
        general.build_leg(materials, side, True)
        general.build_leg(materials, side, False)
    general.build_tail(materials)
    general.build_unit_markers(materials)


def build_melee_details(materials):
    navy = materials["navy"]
    ivory = materials["ivory"]
    gold = materials["gold"]
    cyan = materials["cyan"]
    joint = materials["joint"]

    general.profile_plate(
        "Melee_Ram_Face",
        [(-0.42, -0.3), (0.42, -0.3), (0.52, -0.02),
         (0.3, 0.33), (0.0, 0.52), (-0.3, 0.33), (-0.52, -0.02)],
        0.13, (0.0, -2.25, 1.58), ivory,
        rotation=(math.radians(90), 0.0, 0.0),
        bevel=0.055, bone="head")
    general.profile_plate(
        "Melee_Ram_Gold_Crest",
        [(-0.25, -0.23), (0.25, -0.23), (0.32, 0.0),
         (0.16, 0.25), (0.0, 0.39), (-0.16, 0.25), (-0.32, 0.0)],
        0.035, (0.0, -2.325, 1.58), gold,
        rotation=(math.radians(90), 0.0, 0.0),
        bevel=0.016, bone="head")

    for side in (-1, 1):
        side_name = "L" if side < 0 else "R"
        horn_points = (
            (side * 0.37, -1.88, 1.76),
            (side * 0.58, -2.2, 1.67),
            (side * 0.69, -2.52, 1.56),
            (side * 0.57, -2.8, 1.5),
        )
        for index in range(len(horn_points) - 1):
            general.cylinder_between(
                "Melee_Ram_Horn_%s_%02d" % (side_name, index),
                horn_points[index], horn_points[index + 1],
                0.105 - index * 0.022, gold, vertices=20,
                bevel=0.018, bone="head", taper=0.7)

        general.profile_plate(
            "Melee_Shoulder_Shield_%s" % side_name,
            [(-0.42, -0.46), (0.13, -0.52), (0.45, -0.22),
             (0.4, 0.28), (0.1, 0.53), (-0.36, 0.43),
             (-0.5, 0.02)],
            0.16, (side * 1.36, -0.54, 1.67), ivory,
            rotation=(0.0, side * math.radians(90), 0.0),
            bevel=0.06, bone="body")
        general.spiral_tube(
            "Melee_Shoulder_Spiral_%s" % side_name,
            side * 1.46, -0.56, 1.7, 0.31, gold,
            turns=1.72, tube_radius=0.035, bone="body")
        general.curve_tube(
            "Melee_Shoulder_Energy_%s" % side_name,
            [(side * 1.47, -0.84, 1.47),
             (side * 1.49, -0.56, 1.36),
             (side * 1.46, -0.25, 1.5)],
            0.024, cyan, bone="body")

        leg_bone = "leg_lf" if side < 0 else "leg_rf"
        general.profile_plate(
            "Melee_Foreleg_Bracer_%s" % side_name,
            [(-0.25, -0.25), (0.15, -0.28), (0.28, -0.05),
             (0.2, 0.31), (-0.08, 0.4), (-0.28, 0.18)],
            0.1, (side * 0.95, -0.9, 0.63), ivory,
            rotation=(0.0, side * math.radians(90), 0.0),
            bevel=0.04, bone=leg_bone)
        general.cylinder_between(
            "Melee_Foreleg_Impact_Piston_%s" % side_name,
            (side * 0.88, -0.82, 0.82),
            (side * 0.83, -1.0, 0.28),
            0.052, gold, vertices=16, bevel=0.015,
            bone=leg_bone, taper=0.72)

    general.torus(
        "Melee_Chest_Impact_Ring", (0.0, -1.34, 1.35),
        0.3, 0.055, gold, rotation=(math.radians(90), 0.0, 0.0),
        scale=(1.15, 1.0, 1.0), bone="body")
    general.sphere(
        "Melee_Chest_Impact_Core", (0.0, -1.39, 1.35),
        (0.14, 0.05, 0.14), cyan, segments=24, rings=12, bone="body")
    general.profile_plate(
        "Melee_Dorsal_Guard",
        [(-0.33, -0.45), (0.33, -0.45), (0.43, -0.08),
         (0.26, 0.46), (0.0, 0.62), (-0.26, 0.46), (-0.43, -0.08)],
        0.1, (0.0, 0.25, 2.25), navy,
        bevel=0.045, bone="body")
    general.cylinder_between(
        "Melee_Dorsal_Shock_Rod",
        (0.0, -0.18, 2.29), (0.0, 0.77, 2.29),
        0.045, joint, vertices=16, bevel=0.014, bone="body")


def build_ranged_details(materials):
    navy = materials["navy"]
    ivory = materials["ivory"]
    gold = materials["gold"]
    cyan = materials["cyan"]
    joint = materials["joint"]

    general.torus(
        "Ranged_Turret_Bearing", (0.0, 0.02, 2.22),
        0.42, 0.075, joint, scale=(1.0, 1.18, 1.0), bone="body")
    general.torus(
        "Ranged_Turret_Gold_Race", (0.0, 0.0, 2.27),
        0.34, 0.035, gold, scale=(1.0, 1.16, 1.0), bone="body")
    general.wedge(
        "Ranged_Turret_Armor", (0.0, -0.06, 2.38),
        0.82, 0.38, 0.29, 0.2, 0.15, ivory,
        rotation=(math.radians(-3), 0.0, 0.0),
        bevel=0.065, bone="body")
    general.profile_plate(
        "Ranged_Turret_Crown",
        [(-0.31, -0.38), (0.31, -0.38), (0.39, -0.05),
         (0.22, 0.37), (0.0, 0.5), (-0.22, 0.37), (-0.39, -0.05)],
        0.07, (0.0, -0.05, 2.57), navy,
        bevel=0.035, bone="body")
    general.cylinder_between(
        "Ranged_Main_Barrel_Sleeve",
        (0.0, -0.35, 2.4), (0.0, -1.47, 2.45),
        0.12, navy, vertices=24, bevel=0.025, bone="body",
        taper=0.78)
    general.cylinder_between(
        "Ranged_Main_Barrel_Gold_Jacket",
        (0.0, -1.25, 2.44), (0.0, -1.85, 2.48),
        0.095, gold, vertices=22, bevel=0.018, bone="body",
        taper=0.82)
    general.cylinder_between(
        "Ranged_Main_Barrel_Energy_Bore",
        (0.0, -1.68, 2.48), (0.0, -2.16, 2.51),
        0.062, cyan, vertices=20, bevel=0.012, bone="body",
        taper=0.7)

    for side in (-1, 1):
        side_name = "L" if side < 0 else "R"
        general.profile_plate(
            "Ranged_Sensor_Wing_%s" % side_name,
            [(-0.2, -0.38), (0.2, -0.38), (0.31, -0.03),
             (0.17, 0.42), (0.0, 0.58), (-0.22, 0.35), (-0.3, 0.0)],
            0.065, (side * 0.58, -0.18, 2.45), ivory,
            rotation=(0.0, side * math.radians(74), side * math.radians(8)),
            bevel=0.03, bone="body")
        general.curve_tube(
            "Ranged_Sensor_Energy_%s" % side_name,
            [(side * 0.59, -0.48, 2.38),
             (side * 0.68, -0.2, 2.54),
             (side * 0.63, 0.18, 2.48)],
            0.02, cyan, bone="body")
        general.wedge(
            "Ranged_Power_Pod_%s" % side_name,
            (side * 0.65, 0.34, 2.17), 0.78, 0.2, 0.16,
            0.19, 0.14, navy, rotation=(math.radians(3), 0.0, 0.0),
            bevel=0.045, bone="body")
        general.torus(
            "Ranged_Power_Ring_%s" % side_name,
            (side * 0.66, 0.1, 2.2), 0.16, 0.035, gold,
            rotation=(math.radians(90), 0.0, 0.0),
            scale=(1.0, 1.0, 1.15), bone="body")
        general.sphere(
            "Ranged_Power_Core_%s" % side_name,
            (side * 0.66, 0.07, 2.2), (0.07, 0.04, 0.09), cyan,
            segments=20, rings=10, bone="body")
        general.profile_plate(
            "Ranged_Rear_Stabilizer_%s" % side_name,
            [(-0.18, -0.42), (0.18, -0.42), (0.28, -0.04),
             (0.12, 0.48), (-0.11, 0.57), (-0.27, 0.1)],
            0.07, (side * 0.72, 0.92, 2.07), gold,
            rotation=(0.0, side * math.radians(72), 0.0),
            bevel=0.03, bone="body")

    general.sphere(
        "Ranged_Targeting_Core", (0.0, -0.48, 2.56),
        (0.11, 0.055, 0.075), cyan, segments=24, rings=12, bone="body")


def build_balanced_details(materials):
    navy = materials["navy"]
    ivory = materials["ivory"]
    gold = materials["gold"]
    cyan = materials["cyan"]
    joint = materials["joint"]

    turret_x = -0.34
    general.torus(
        "Balanced_Compact_Turret_Bearing", (turret_x, 0.03, 2.2),
        0.29, 0.055, joint, scale=(1.0, 1.12, 1.0), bone="body")
    general.wedge(
        "Balanced_Compact_Turret", (turret_x, -0.07, 2.33),
        0.62, 0.29, 0.22, 0.16, 0.12, ivory,
        rotation=(math.radians(-3), 0.0, math.radians(-4)),
        bevel=0.055, bone="body")
    general.cylinder_between(
        "Balanced_Compact_Barrel",
        (turret_x, -0.32, 2.36), (turret_x - 0.03, -1.35, 2.4),
        0.08, navy, vertices=20, bevel=0.02, bone="body",
        taper=0.72)
    general.cylinder_between(
        "Balanced_Compact_Energy_Bore",
        (turret_x - 0.03, -1.16, 2.4),
        (turret_x - 0.04, -1.63, 2.42),
        0.045, cyan, vertices=18, bevel=0.012, bone="body",
        taper=0.64)
    general.profile_plate(
        "Balanced_Turret_Gold_Inlay",
        [(-0.19, -0.26), (0.19, -0.26), (0.25, 0.02),
         (0.11, 0.31), (-0.12, 0.34), (-0.25, 0.02)],
        0.025, (turret_x, -0.08, 2.51), gold,
        bevel=0.014, bone="body")

    general.profile_plate(
        "Balanced_Right_Jaw_Guard",
        [(-0.25, -0.34), (0.14, -0.4), (0.32, -0.1),
         (0.21, 0.34), (-0.08, 0.46), (-0.3, 0.16)],
        0.095, (0.5, -1.75, 1.56), ivory,
        rotation=(0.0, math.radians(90), 0.0),
        bevel=0.04, bone="head")
    horn_points = (
        (0.34, -1.95, 1.5),
        (0.5, -2.23, 1.42),
        (0.48, -2.52, 1.35),
    )
    for index in range(len(horn_points) - 1):
        general.cylinder_between(
            "Balanced_Right_Jaw_Tusk_%02d" % index,
            horn_points[index], horn_points[index + 1],
            0.075 - index * 0.018, gold, vertices=18,
            bevel=0.014, bone="head", taper=0.62)
    general.curve_tube(
        "Balanced_Right_Jaw_Energy",
        [(0.5, -1.94, 1.7), (0.55, -2.1, 1.53),
         (0.5, -2.3, 1.42)],
        0.021, cyan, bone="head")

    general.profile_plate(
        "Balanced_Left_Sensor_Shield",
        [(-0.3, -0.37), (0.15, -0.42), (0.37, -0.14),
         (0.29, 0.28), (0.03, 0.48), (-0.32, 0.34),
         (-0.43, -0.02)],
        0.1, (-1.28, -0.54, 1.69), navy,
        rotation=(0.0, -math.radians(90), 0.0),
        bevel=0.045, bone="body")
    general.spiral_tube(
        "Balanced_Left_Shoulder_Spiral",
        -1.35, -0.56, 1.7, 0.25, gold,
        turns=1.62, tube_radius=0.028, bone="body")
    general.profile_plate(
        "Balanced_Left_Sensor_Fin",
        [(-0.17, -0.32), (0.17, -0.32), (0.25, 0.0),
         (0.08, 0.44), (-0.12, 0.52), (-0.25, 0.08)],
        0.06, (-1.18, -0.44, 2.05), ivory,
        rotation=(0.0, -math.radians(76), math.radians(-8)),
        bevel=0.028, bone="body")
    general.sphere(
        "Balanced_Left_Sensor_Core", (-1.35, -0.63, 1.75),
        (0.055, 0.09, 0.09), cyan, segments=20, rings=10, bone="body")

    general.profile_plate(
        "Balanced_Right_Foreleg_Bracer",
        [(-0.23, -0.25), (0.14, -0.29), (0.27, -0.03),
         (0.17, 0.33), (-0.1, 0.4), (-0.27, 0.16)],
        0.09, (0.94, -0.9, 0.63), ivory,
        rotation=(0.0, math.radians(90), 0.0),
        bevel=0.038, bone="leg_rf")
    general.cylinder_between(
        "Balanced_Right_Foreleg_Piston",
        (0.87, -0.82, 0.82), (0.83, -1.0, 0.28),
        0.045, gold, vertices=16, bevel=0.013,
        bone="leg_rf", taper=0.7)
    general.curve_tube(
        "Balanced_Dorsal_Energy_Bridge",
        [(-0.55, 0.52, 2.2), (-0.1, 0.68, 2.28),
         (0.42, 0.54, 2.2)],
        0.022, cyan, bone="body")


def add_variant_details(role, materials):
    if role == "Melee":
        build_melee_details(materials)
    elif role == "Ranged":
        build_ranged_details(materials)
    elif role == "Balanced":
        build_balanced_details(materials)
    else:
        raise ValueError("Unsupported Haetae upgrade role: " + role)


def generate_variant(spec):
    role = spec["role"]
    paths = role_paths(role)
    begin_variant()
    materials = general.build_materials()
    build_common_body(materials)
    add_variant_details(role, materials)

    armature = general.build_armature()
    armature.name = "Haetae_%s_Rig" % role
    armature.data.name = "Haetae_%s_Rig" % role
    armature["asset_id"] = spec["asset_id"]
    armature["variant_role"] = role
    armature["silhouette_signature"] = spec["signature"]
    armature["source_recipe"] = "ArtSource/Haetae/create_haetae_upgrades.py"
    body = general.consolidate_meshes(armature)
    body.name = "Haetae_%s_Body" % role

    general.export_fbx(paths["lod0"], armature)
    general.export_fbx(paths["lod1"], armature, decimate_ratio=0.52)
    general.PREVIEW_PATH = paths["preview"]
    general.render_preview()
    bpy.ops.wm.save_as_mainfile(filepath=str(paths["blend"]), compress=True)

    source_vertices = sum(
        len(obj.data.vertices) for obj in general.mesh_objects)
    polygon_materials = {}
    for polygon in body.data.polygons:
        polygon_materials[polygon.material_index] = (
            polygon_materials.get(polygon.material_index, 0) + 1)
    print("HAETAE_UPGRADE_ROLE=%s" % role)
    print("ASSET_ID=%s" % spec["asset_id"])
    print("SOURCE_VERTICES=%d" % source_vertices)
    print("BODY_MATERIAL_POLYGONS=%s" % polygon_materials)
    print("LOD0=%s" % paths["lod0"])
    print("LOD1=%s" % paths["lod1"])
    print("PREVIEW=%s" % paths["preview"])
    print("BLEND=%s" % paths["blend"])
    return paths


def aim_at(obj, target):
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def render_gallery(variant_paths):
    general.reset_scene()
    general.mesh_objects.clear()
    general.model_objects.clear()

    offsets = (-3.15, 0.0, 3.15)
    for index, paths in enumerate(variant_paths):
        before = set(bpy.context.scene.objects)
        bpy.ops.import_scene.fbx(filepath=str(paths["lod0"]))
        imported = [
            obj for obj in bpy.context.scene.objects if obj not in before]
        group = bpy.data.objects.new(
            VARIANTS[index]["role"] + "_Gallery_Root", None)
        bpy.context.collection.objects.link(group)
        for obj in imported:
            if obj.parent is not None and obj.parent in imported:
                continue
            world = obj.matrix_world.copy()
            obj.parent = group
            obj.matrix_world = world
        group.location = (offsets[index], 0.0, 0.0)
        group.scale = (0.9, 0.9, 0.9)

    bpy.ops.mesh.primitive_plane_add(size=24, location=(0.0, 0.25, 0.0))
    floor = bpy.context.object
    floor.name = "Gallery Floor"
    floor_material = general.make_material(
        "MAT_GalleryFloor", (0.055, 0.06, 0.07), 0.05, 0.72)
    floor.data.materials.append(floor_material)

    bpy.ops.object.camera_add(location=(8.8, -13.4, 5.8))
    camera = bpy.context.object
    camera.name = "Gallery Camera"
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 9.55
    aim_at(camera, (0.0, 0.0, 1.35))
    bpy.context.scene.camera = camera

    lights = (
        ("Gallery Key", (4.5, -5.5, 7.5), 1850, 5.0),
        ("Gallery Fill", (-5.5, -2.0, 4.5), 1300, 4.5),
        ("Gallery Rim", (0.0, 5.5, 6.5), 1650, 4.0),
    )
    for name, location, energy, size in lights:
        bpy.ops.object.light_add(type="AREA", location=location)
        light = bpy.context.object
        light.name = name
        light.data.energy = energy
        light.data.size = size
        aim_at(light, (0.0, 0.0, 1.4))

    scene = bpy.context.scene
    scene.world.color = (0.012, 0.016, 0.025)
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.render.resolution_x = 1920
    scene.render.resolution_y = 1080
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.filepath = str(GALLERY_PATH)
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.view_settings.exposure = 0.55
    bpy.ops.render.render(write_still=True)
    print("GALLERY=%s" % GALLERY_PATH)


def report_fbx_metrics(variant_paths):
    for spec, paths in zip(VARIANTS, variant_paths):
        counts = {}
        for key in ("lod0", "lod1"):
            general.reset_scene()
            bpy.ops.import_scene.fbx(filepath=str(paths[key]))
            counts[key] = sum(
                len(obj.data.vertices)
                for obj in bpy.context.scene.objects
                if obj.type == "MESH")
        ratio = counts["lod1"] / counts["lod0"] * 100.0
        print(
            "FBX_METRICS_%s=LOD0:%d,LOD1:%d,RATIO:%.2f%%" %
            (spec["role"], counts["lod0"], counts["lod1"], ratio))


def main():
    MODEL_DIR.mkdir(parents=True, exist_ok=True)
    generated = []
    for spec in VARIANTS:
        generated.append(generate_variant(spec))
    render_gallery(generated)
    report_fbx_metrics(generated)
    print("HAETAE_UPGRADES_BUILD_COMPLETE")


if __name__ == "__main__":
    main()
