"""Create the three project-owned production zombie models.

Run with Blender 4.5 LTS:
    blender --background --factory-startup --python create_zombie_models.py

The checked-in FBX files are runtime inputs. Blender is only required when
regenerating the editable sources.
"""

from pathlib import Path
import math
import sys

import bpy
from mathutils import Vector


SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_DIR = SCRIPT_DIR.parent.parent
MODEL_DIR = PROJECT_DIR / "Assets" / "Game" / "Art" / "Models" / "Zombies"
HAETAE_SOURCE_DIR = SCRIPT_DIR.parent / "Haetae"
sys.path.insert(0, str(HAETAE_SOURCE_DIR))
import create_haetae_general as geo


ROLES = {
    "Runner": {
        "asset_id": "enemy.runner",
        "signature": "zombie.authored.runner.pursuit",
        "lod_ratio": 0.38,
    },
    "Bruiser": {
        "asset_id": "enemy.bruiser",
        "signature": "zombie.authored.bruiser.siege",
        "lod_ratio": 0.36,
    },
    "Ripper": {
        "asset_id": "enemy.ripper",
        "signature": "zombie.authored.ripper.scythe",
        "lod_ratio": 0.38,
    },
}

REQUIRED_BONES = [
    "hips", "spine", "chest", "neck", "head",
    "upper_arm_l", "lower_arm_l", "hand_l",
    "upper_arm_r", "lower_arm_r", "hand_r",
    "thigh_l", "shin_l", "foot_l",
    "thigh_r", "shin_r", "foot_r",
]


def clear_scene():
    geo.reset_scene()
    geo.mesh_objects.clear()
    geo.model_objects.clear()


def create_collection(name):
    collection = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(collection)
    bpy.context.view_layer.active_layer_collection = (
        bpy.context.view_layer.layer_collection.children[name])


def build_materials():
    # Deliberately stylized: readable infected tissue without realistic gore.
    return {
        "flesh": geo.make_material(
            "MAT_ZombieFlesh", (0.19, 0.25, 0.22), 0.02, 0.62),
        "armor": geo.make_material(
            "MAT_ZombieArmor", (0.095, 0.075, 0.07), 0.62, 0.34),
        "tissue": geo.make_material(
            "MAT_ZombieTissue", (0.025, 0.035, 0.034), 0.08, 0.48),
        "corruption": geo.make_material(
            "MAT_ZombieCorruption", (0.34, 0.012, 0.018), 0.04, 0.3,
            emission=(1.0, 0.025, 0.012), strength=4.2),
        "bone": geo.make_material(
            "MAT_ZombieBone", (0.57, 0.51, 0.37), 0.08, 0.5),
    }


def dense_sphere(name, location, scale, material, bone):
    return geo.sphere(
        name, location, scale, material, segments=32, rings=16, bone=bone)


def joint(name, location, radius, material, bone):
    return geo.sphere(
        name, location, (radius, radius, radius), material,
        segments=24, rings=12, bone=bone)


def plate(name, location, width, height, material, bone, asymmetry=0.0):
    points = [
        (-width * 0.52, -height * 0.48),
        (width * (0.44 + asymmetry), -height * 0.55),
        (width * 0.58, -height * 0.08),
        (width * 0.42, height * 0.52),
        (-width * 0.38, height * 0.58),
        (-width * (0.56 - asymmetry), height * 0.06),
    ]
    return geo.profile_plate(
        name, points, 0.055, location, material,
        rotation=(math.radians(90), 0.0, 0.0),
        bevel=0.025, bone=bone)


def spike(name, start, end, radius, material, bone):
    return geo.cylinder_between(
        name, start, end, radius, material, vertices=20,
        bevel=0.012, bone=bone, taper=0.05)


def segment(name, start, end, radius, material, bone, taper=0.78):
    geo.cylinder_between(
        name + "_Structure", start, end, radius, material, vertices=24,
        bevel=0.025, bone=bone, taper=taper)
    midpoint = Vector(start).lerp(Vector(end), 0.5)
    length = (Vector(end) - Vector(start)).length
    dense_sphere(
        name + "_Muscle",
        midpoint,
        (radius * 1.2, radius * 0.98, max(radius * 1.25, length * 0.29)),
        material,
        bone)


def build_organic_flesh_shell(role, pose, material, chest_scale,
                              belly_scale, hip_scale, limb_radius):
    """Fuse overlapping anatomy into one sculpt-like continuous surface."""
    mesh_start = len(geo.mesh_objects)
    model_start = len(geo.model_objects)

    def shell_sphere(name, location, scale, bone):
        geo.sphere(
            name, location, scale, material, segments=20, rings=10, bone=bone)

    shell_sphere("Shell_Pelvis", pose["hips"], hip_scale, "hips")
    shell_sphere("Shell_Abdomen", pose["spine"], belly_scale, "spine")
    shell_sphere("Shell_Ribcage", pose["chest"], chest_scale, "chest")
    shell_sphere(
        "Shell_Left_Pectoral",
        (-chest_scale[0] * 0.46, -0.19, pose["chest"][2] + 0.025),
        (chest_scale[0] * 0.6, chest_scale[1] * 0.7,
         chest_scale[2] * 0.57), "chest")
    shell_sphere(
        "Shell_Right_Pectoral",
        (chest_scale[0] * 0.46, -0.19, pose["chest"][2] + 0.025),
        (chest_scale[0] * 0.6, chest_scale[1] * 0.7,
         chest_scale[2] * 0.57), "chest")
    shell_sphere(
        "Shell_Neck", pose["neck"],
        (limb_radius * 1.35, limb_radius * 1.25, limb_radius * 1.45),
        "neck")
    head_width = 0.22 if role != "Bruiser" else 0.28
    shell_sphere(
        "Shell_Cranium", pose["head"],
        (head_width, head_width * 0.9,
         0.25 if role != "Ripper" else 0.28), "head")
    shell_sphere(
        "Shell_Muzzle",
        (0.0, pose["head"][1] - 0.14, pose["head"][2] - 0.13),
        (head_width * 0.82, head_width * 0.7, head_width * 0.48), "head")

    for side, suffix in ((-1, "l"), (1, "r")):
        arm = pose["arm_" + suffix]
        leg = pose["leg_" + suffix]
        arm_bulk = 1.32 if role == "Bruiser" else 1.0
        geo.cylinder_between(
            "Shell_UpperArm_" + suffix, arm[0], arm[1],
            limb_radius * arm_bulk, material, vertices=18, bevel=0.0,
            bone="upper_arm_" + suffix, taper=0.82)
        shell_sphere(
            "Shell_Elbow_" + suffix, arm[1],
            (limb_radius * 1.2 * arm_bulk,) * 3, "lower_arm_" + suffix)
        geo.cylinder_between(
            "Shell_LowerArm_" + suffix, arm[1], arm[2],
            limb_radius * (1.22 if role == "Bruiser" else 0.88) * arm_bulk,
            material, vertices=18, bevel=0.0, bone="lower_arm_" + suffix,
            taper=0.74)
        shell_sphere(
            "Shell_Hand_" + suffix, arm[2],
            ((0.19 if role == "Bruiser" else 0.12) * arm_bulk,
             (0.15 if role == "Bruiser" else 0.095) * arm_bulk,
             (0.18 if role == "Bruiser" else 0.13) * arm_bulk),
            "hand_" + suffix)
        geo.cylinder_between(
            "Shell_Thigh_" + suffix, leg[0], leg[1],
            limb_radius * 1.24, material, vertices=18, bevel=0.0,
            bone="thigh_" + suffix, taper=0.84)
        shell_sphere(
            "Shell_Knee_" + suffix, leg[1],
            (limb_radius * 1.1,) * 3, "shin_" + suffix)
        geo.cylinder_between(
            "Shell_Shin_" + suffix, leg[1], leg[2],
            limb_radius * (0.78 if role == "Runner" else 0.92),
            material, vertices=18, bevel=0.0, bone="shin_" + suffix,
            taper=0.68)
        shell_sphere(
            "Shell_Ankle_" + suffix, leg[2],
            (limb_radius * 0.82,) * 3, "foot_" + suffix)

    components = list(geo.mesh_objects[mesh_start:])
    bpy.ops.object.select_all(action="DESELECT")
    for obj in components:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = components[0]
    bpy.ops.object.join()
    shell = bpy.context.object
    shell.name = "Zombie_%s_Organic_Flesh" % role
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    remesh = shell.modifiers.new("Organic Voxel Sculpt", "REMESH")
    if hasattr(remesh, "mode"):
        remesh.mode = "VOXEL"
    if hasattr(remesh, "voxel_size"):
        remesh.voxel_size = 0.032 if role != "Bruiser" else 0.038
    if hasattr(remesh, "octree_depth"):
        remesh.octree_depth = 7
    if hasattr(remesh, "scale"):
        remesh.scale = 0.94
    if hasattr(remesh, "use_remove_disconnected"):
        remesh.use_remove_disconnected = False
    if hasattr(remesh, "use_smooth_shade"):
        remesh.use_smooth_shade = True
    bpy.context.view_layer.objects.active = shell
    bpy.ops.object.modifier_apply(modifier=remesh.name)

    smooth = shell.modifiers.new("Anatomy Surface Relax", "SMOOTH")
    smooth.factor = 0.32
    smooth.iterations = 3
    bpy.ops.object.modifier_apply(modifier=smooth.name)
    for polygon in shell.data.polygons:
        polygon.use_smooth = True
    shell.data.materials.clear()
    shell.data.materials.append(material)
    shell["rig_bone"] = "spine"
    geo.mesh_objects[:] = geo.mesh_objects[:mesh_start] + [shell]
    geo.model_objects[:] = geo.model_objects[:model_start] + [shell]


def build_rig(role, pose):
    rig_name = "Zombie_%s_Rig" % role
    data = bpy.data.armatures.new(rig_name)
    armature = bpy.data.objects.new(rig_name, data)
    bpy.context.collection.objects.link(armature)
    geo.model_objects.append(armature)
    bpy.context.view_layer.objects.active = armature
    armature.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")

    def bone(name, head, tail, parent=None):
        created = data.edit_bones.new(name)
        created.head = head
        created.tail = tail
        if parent is not None:
            created.parent = data.edit_bones.get(parent)
        return created

    bone("root", (0.0, 0.0, -1.0), pose["hips"])
    bone("hips", pose["hips"], pose["spine"], "root")
    bone("spine", pose["spine"], pose["chest"], "hips")
    bone("chest", pose["chest"], pose["neck"], "spine")
    bone("neck", pose["neck"], pose["head"], "chest")
    bone("head", pose["head"], Vector(pose["head"]) + Vector((0.0, -0.12, 0.2)), "neck")
    for side, suffix in ((-1, "l"), (1, "r")):
        arm = pose["arm_" + suffix]
        leg = pose["leg_" + suffix]
        bone("upper_arm_" + suffix, arm[0], arm[1], "chest")
        bone("lower_arm_" + suffix, arm[1], arm[2], "upper_arm_" + suffix)
        bone("hand_" + suffix, arm[2], arm[3], "lower_arm_" + suffix)
        bone("thigh_" + suffix, leg[0], leg[1], "hips")
        bone("shin_" + suffix, leg[1], leg[2], "thigh_" + suffix)
        bone("foot_" + suffix, leg[2], leg[3], "shin_" + suffix)

    bpy.ops.object.mode_set(mode="OBJECT")
    armature["asset_id"] = ROLES[role]["asset_id"]
    armature["silhouette_signature"] = ROLES[role]["signature"]
    armature["source_recipe"] = "ArtSource/Zombies/create_zombie_models.py"
    armature["forward_axis"] = "-Y"
    armature["detail_revision"] = 1
    armature["gameplay_owner"] = "ZombieActor capsule root"
    armature.show_in_front = True
    armature.select_set(False)
    return armature


def runner_pose():
    return {
        "hips": (0.0, 0.08, -0.18),
        "spine": (0.0, -0.02, 0.16),
        "chest": (0.0, -0.16, 0.51),
        "neck": (0.0, -0.28, 0.78),
        "head": (0.0, -0.4, 0.98),
        "arm_l": [(-0.34, -0.16, 0.56), (-0.48, -0.38, 0.2),
                  (-0.43, -0.66, -0.13), (-0.39, -0.8, -0.26)],
        "arm_r": [(0.34, -0.16, 0.56), (0.48, -0.38, 0.2),
                  (0.43, -0.66, -0.13), (0.39, -0.8, -0.26)],
        "leg_l": [(-0.18, 0.04, -0.18), (-0.23, -0.01, -0.55),
                  (-0.26, -0.19, -0.91), (-0.26, -0.47, -0.98)],
        "leg_r": [(0.18, 0.04, -0.18), (0.23, 0.09, -0.55),
                  (0.26, -0.09, -0.91), (0.26, -0.37, -0.98)],
    }


def bruiser_pose():
    return {
        "hips": (0.0, 0.04, -0.28),
        "spine": (0.0, 0.0, 0.02),
        "chest": (0.0, -0.08, 0.42),
        "neck": (0.0, -0.22, 0.63),
        "head": (0.0, -0.34, 0.78),
        "arm_l": [(-0.58, -0.07, 0.43), (-0.78, -0.18, 0.08),
                  (-0.78, -0.39, -0.34), (-0.73, -0.53, -0.52)],
        "arm_r": [(0.58, -0.07, 0.43), (0.82, -0.13, 0.1),
                  (0.82, -0.36, -0.37), (0.78, -0.53, -0.54)],
        "leg_l": [(-0.32, 0.02, -0.28), (-0.4, 0.02, -0.62),
                  (-0.46, -0.05, -0.91), (-0.5, -0.33, -0.98)],
        "leg_r": [(0.32, 0.02, -0.28), (0.4, 0.02, -0.62),
                  (0.46, -0.05, -0.91), (0.5, -0.33, -0.98)],
    }


def ripper_pose():
    return {
        "hips": (0.0, 0.05, -0.16),
        "spine": (0.0, 0.0, 0.2),
        "chest": (0.0, -0.08, 0.62),
        "neck": (0.0, -0.18, 0.92),
        "head": (0.0, -0.28, 1.16),
        "arm_l": [(-0.42, -0.08, 0.68), (-0.66, -0.24, 0.31),
                  (-0.78, -0.45, -0.06), (-0.84, -0.55, -0.3)],
        "arm_r": [(0.42, -0.08, 0.68), (0.66, -0.24, 0.31),
                  (0.78, -0.45, -0.06), (0.84, -0.55, -0.3)],
        "leg_l": [(-0.22, 0.02, -0.16), (-0.29, 0.01, -0.56),
                  (-0.25, -0.08, -0.92), (-0.27, -0.38, -0.98)],
        "leg_r": [(0.22, 0.02, -0.16), (0.29, 0.01, -0.56),
                  (0.25, -0.08, -0.92), (0.27, -0.38, -0.98)],
    }


def build_shared_anatomy(role, pose, materials):
    flesh = materials["flesh"]
    armor = materials["armor"]
    tissue = materials["tissue"]
    corruption = materials["corruption"]
    bone = materials["bone"]

    if role == "Runner":
        chest_scale, belly_scale, hip_scale = (
            (0.38, 0.27, 0.34), (0.28, 0.22, 0.3), (0.31, 0.23, 0.22))
        limb_radius = 0.105
        joint_radius = 0.12
    elif role == "Bruiser":
        chest_scale, belly_scale, hip_scale = (
            (0.64, 0.41, 0.39), (0.5, 0.35, 0.31), (0.49, 0.34, 0.26))
        limb_radius = 0.17
        joint_radius = 0.18
    else:
        chest_scale, belly_scale, hip_scale = (
            (0.44, 0.28, 0.42), (0.33, 0.24, 0.35), (0.35, 0.26, 0.25))
        limb_radius = 0.13
        joint_radius = 0.14

    build_organic_flesh_shell(
        role, pose, flesh, chest_scale, belly_scale, hip_scale, limb_radius)

    joint("Neck_Tissue", pose["neck"], joint_radius * 0.82, tissue, "neck")
    dense_sphere(
        "Jaw_Mass",
        (0.0, pose["head"][1] - 0.12, pose["head"][2] - 0.13),
        (0.17 if role != "Bruiser" else 0.22, 0.12, 0.09),
        tissue, "head")

    plate(
        "Fractured_Chest_Armor", (-chest_scale[0] * 0.08,
         -chest_scale[1] - 0.035, pose["chest"][2] + 0.035),
        chest_scale[0] * 1.25, chest_scale[2] * 1.05,
        armor, "chest", 0.12 if role == "Bruiser" else -0.08)
    plate(
        "Pelvis_Armor", (0.0, -hip_scale[1] - 0.025, pose["hips"][2]),
        hip_scale[0] * 1.25, hip_scale[2] * 0.9, armor, "hips")
    plate(
        "Broken_Face_Mask", (-0.045, pose["head"][1] - 0.205,
         pose["head"][2] + 0.045),
        0.22 if role != "Bruiser" else 0.29,
        0.2 if role != "Ripper" else 0.25, armor, "head", -0.16)

    # Glowing sternum and facial focus establish a shared faction language.
    dense_sphere(
        "Sternum_Corruption",
        (0.0, -chest_scale[1] - 0.08, pose["chest"][2] + 0.01),
        (0.1 if role != "Bruiser" else 0.15, 0.055, 0.13),
        corruption, "chest")
    for side in (-1, 1):
        dense_sphere(
            "Eye_Corruption_L" if side < 0 else "Eye_Corruption_R",
            (side * (0.075 if role != "Bruiser" else 0.1),
             pose["head"][1] - 0.205, pose["head"][2] + 0.055),
            (0.035, 0.025, 0.027), corruption, "head")
        for tooth in (-1, 1):
            x = side * 0.07 + tooth * 0.018
            spike(
                "Jaw_Tooth_%s_%s" % ("L" if side < 0 else "R",
                                      "A" if tooth < 0 else "B"),
                (x, pose["head"][1] - 0.24, pose["head"][2] - 0.13),
                (x + side * 0.012, pose["head"][1] - 0.29,
                 pose["head"][2] - 0.23),
                0.016, bone, "head")

    # Each arm and leg receives distinct named deformation ownership even
    # though this revision retains the established transform-driven motion.
    for side, suffix in ((-1, "l"), (1, "r")):
        arm = pose["arm_" + suffix]
        leg = pose["leg_" + suffix]
        arm_scale = 1.45 if role == "Bruiser" else 1.0
        joint("Elbow_" + suffix, arm[1], joint_radius * arm_scale,
              tissue, "lower_arm_" + suffix)
        dense_sphere(
            "Exposed_Hand_Tissue_" + suffix, arm[2],
            ((0.13 if role == "Bruiser" else 0.075) * arm_scale,
             (0.1 if role == "Bruiser" else 0.06) * arm_scale,
             (0.12 if role == "Bruiser" else 0.08) * arm_scale),
            tissue, "hand_" + suffix)
        joint("Knee_" + suffix, leg[1], joint_radius * 0.9,
              tissue, "shin_" + suffix)
        geo.wedge(
            "Foot_" + suffix, leg[3], 0.33 if role != "Bruiser" else 0.4,
            0.13 if role != "Bruiser" else 0.19, 0.1,
            0.1, 0.055, tissue,
            rotation=(math.radians(2), 0.0, 0.0),
            bevel=0.035, bone="foot_" + suffix)

        plate(
            "Shoulder_Armor_" + suffix,
            (arm[0][0], -0.19, arm[0][2] + 0.02),
            0.28 if role != "Bruiser" else 0.48,
            0.25 if role != "Bruiser" else 0.38,
            armor, "upper_arm_" + suffix, side * 0.05)
        plate(
            "Shin_Armor_" + suffix,
            (leg[2][0], leg[2][1] - 0.12, leg[2][2] + 0.13),
            0.18 if role != "Bruiser" else 0.25,
            0.28, armor, "shin_" + suffix, side * 0.03)

        for claw_index in (-1, 0, 1):
            claw_x = leg[3][0] + claw_index * (0.065 if role != "Bruiser" else 0.09)
            spike(
                "Toe_Claw_%s_%d" % (suffix, claw_index + 2),
                (claw_x, leg[3][1] - 0.08, -0.94),
                (claw_x, leg[3][1] - 0.27, -0.99),
                0.03 if role != "Bruiser" else 0.045,
                bone, "foot_" + suffix)

    # Layered surface nodes and veins replace the clean primitive look.
    node_locations = [
        (-0.22, -0.22, 0.3), (0.24, -0.21, 0.38),
        (-0.18, -0.18, 0.08), (0.2, -0.17, 0.0),
        (-0.11, -0.2, -0.17), (0.13, -0.19, -0.12),
        (-0.27, -0.06, 0.55), (0.29, -0.05, 0.52),
    ]
    spread = 1.5 if role == "Bruiser" else 1.0
    for index, location in enumerate(node_locations):
        dense_sphere(
            "Infection_Node_%02d" % index,
            (location[0] * spread, location[1], location[2]),
            (0.055 + (index % 3) * 0.012,
             0.035 + (index % 2) * 0.008,
             0.06 + ((index + 1) % 3) * 0.01),
            corruption if index % 3 == 0 else tissue,
            "chest" if location[2] > 0.2 else "spine")
    geo.curve_tube(
        "Corruption_Vein_L",
        [(-0.04, -0.32, pose["chest"][2] + 0.12),
         (-0.18 * spread, -0.31, pose["spine"][2] + 0.12),
         (-0.12 * spread, -0.28, pose["hips"][2] + 0.04)],
        0.018, corruption, bone="spine")
    geo.curve_tube(
        "Corruption_Vein_R",
        [(0.04, -0.32, pose["chest"][2] + 0.1),
         (0.2 * spread, -0.29, pose["spine"][2] + 0.06),
         (0.1 * spread, -0.27, pose["hips"][2] + 0.02)],
        0.015, corruption, bone="spine")


def build_runner(pose, materials):
    corruption = materials["corruption"]
    bone = materials["bone"]
    armor = materials["armor"]

    # Aerodynamic pursuit silhouette: rearward dorsal spines and long talons.
    spine_roots = [
        (0.0, 0.18, 0.2), (0.0, 0.17, 0.38),
        (0.0, 0.13, 0.58), (0.0, 0.06, 0.76),
    ]
    for index, root in enumerate(spine_roots):
        spike(
            "Runner_Pursuit_Spine_%02d" % index, root,
            (0.0, 0.42 + index * 0.04, root[2] + 0.16 + index * 0.025),
            0.055 - index * 0.006,
            corruption if index % 2 == 0 else bone,
            "spine" if index < 2 else "chest")

    for side, suffix in ((-1, "l"), (1, "r")):
        arm = pose["arm_" + suffix]
        for claw_index in (-1, 0, 1):
            start = (arm[3][0] + claw_index * 0.035, arm[3][1], arm[3][2])
            end = (start[0] + side * claw_index * 0.02, start[1] - 0.22,
                   start[2] - 0.1)
            spike("Runner_Hand_Claw_%s_%d" % (suffix, claw_index + 2),
                  start, end, 0.024, bone, "hand_" + suffix)
        plate(
            "Runner_Forearm_Vane_" + suffix,
            (arm[2][0], arm[2][1] + 0.04, arm[2][2] + 0.06),
            0.17, 0.38, armor, "lower_arm_" + suffix, side * 0.08)


def build_bruiser(pose, materials):
    armor = materials["armor"]
    tissue = materials["tissue"]
    corruption = materials["corruption"]
    bone = materials["bone"]

    # A low fortress-like upper body, with intentional right-side imbalance.
    for side, suffix in ((-1, "l"), (1, "r")):
        shoulder = pose["arm_" + suffix][0]
        dense_sphere(
            "Bruiser_Shoulder_Mass_" + suffix,
            (shoulder[0] * 1.04, shoulder[1], shoulder[2] + 0.02),
            (0.3 + (0.06 if side > 0 else 0.0), 0.28, 0.3),
            corruption if side > 0 else tissue, "upper_arm_" + suffix)
        plate(
            "Bruiser_Layered_Pauldron_" + suffix,
            (shoulder[0], -0.31, shoulder[2] + 0.08),
            0.55 + (0.08 if side > 0 else 0.0), 0.46,
            armor, "upper_arm_" + suffix, side * 0.12)
        hand = pose["arm_" + suffix][3]
        for knuckle in range(4):
            x = hand[0] + (knuckle - 1.5) * 0.075
            dense_sphere(
                "Bruiser_Knuckle_%s_%d" % (suffix, knuckle),
                (x, hand[1] - 0.07, hand[2] - 0.02),
                (0.07, 0.06, 0.065), bone, "hand_" + suffix)

    for layer, z in enumerate((0.1, 0.31, 0.5)):
        plate(
            "Bruiser_Torso_Armor_%02d" % layer,
            (0.0, -0.47 - layer * 0.008, z),
            0.88 - layer * 0.1, 0.28,
            armor, "chest" if layer > 0 else "spine",
            0.1 if layer == 2 else -0.04)

    for index, location in enumerate((
            (0.42, -0.32, 0.44), (0.52, -0.2, 0.34),
            (0.5, -0.1, 0.21), (0.38, -0.22, 0.12))):
        dense_sphere(
            "Bruiser_Asymmetric_Corruption_%02d" % index,
            location, (0.11 + index * 0.01, 0.09, 0.12),
            corruption, "chest")
        if index < 3:
            spike(
                "Bruiser_Mass_Spike_%02d" % index,
                location,
                (location[0] + 0.18, location[1] + 0.08,
                 location[2] + 0.12 - index * 0.03),
                0.045, bone, "chest")


def build_ripper(pose, materials):
    bone = materials["bone"]
    corruption = materials["corruption"]
    armor = materials["armor"]
    tissue = materials["tissue"]

    # Paired scythe forearms are profile-authored blades, not scaled cubes.
    blade_points = [
        (-0.1, -0.6), (0.03, -0.67), (0.1, -0.34),
        (0.1, 0.08), (0.04, 0.52), (-0.08, 0.82),
        (-0.06, 0.36), (-0.12, -0.08),
    ]
    for side, suffix in ((-1, "l"), (1, "r")):
        hand = pose["arm_" + suffix][2]
        geo.profile_plate(
            "Ripper_Scythe_" + suffix,
            [(x * side, y) for x, y in blade_points],
            0.085,
            (hand[0] + side * 0.08, hand[1] - 0.1, hand[2] - 0.16),
            bone,
            rotation=(math.radians(90), 0.0, side * math.radians(10)),
            bevel=0.035, bone="hand_" + suffix)
        geo.curve_tube(
            "Ripper_Blade_Vein_" + suffix,
            [(hand[0], hand[1] - 0.12, hand[2] + 0.14),
             (hand[0] + side * 0.08, hand[1] - 0.17, hand[2] - 0.08),
             (hand[0] + side * 0.12, hand[1] - 0.15, hand[2] - 0.34)],
            0.018, corruption, bone="hand_" + suffix)

    for side, suffix in ((-1, "l"), (1, "r")):
        spike(
            "Ripper_Split_Crest_" + suffix,
            (side * 0.07, -0.25, pose["head"][2] + 0.17),
            (side * 0.2, -0.12, pose["head"][2] + 0.53),
            0.065, bone, "head")
    geo.torus(
        "Ripper_AntiRobot_Core", (0.0, -0.38, pose["chest"][2] + 0.02),
        0.17, 0.038, corruption,
        rotation=(math.radians(90), 0.0, 0.0),
        scale=(1.0, 1.0, 0.82), bone="chest")
    dense_sphere(
        "Ripper_Core_Heart", (0.0, -0.39, pose["chest"][2] + 0.02),
        (0.09, 0.04, 0.09), corruption, "chest")
    for side in (-1, 1):
        plate(
            "Ripper_Rib_Armor_L" if side < 0 else "Ripper_Rib_Armor_R",
            (side * 0.22, -0.31, pose["chest"][2] - 0.02),
            0.24, 0.58, armor, "chest", side * 0.1)
        geo.curve_tube(
            "Ripper_External_Tendon_L" if side < 0 else "Ripper_External_Tendon_R",
            [(side * 0.14, -0.28, pose["neck"][2]),
             (side * 0.3, -0.27, pose["chest"][2] + 0.08),
             (side * 0.2, -0.25, pose["spine"][2])],
            0.022, tissue, bone="chest")


def consolidate(role, armature):
    body = geo.consolidate_meshes(armature)
    body.name = "Zombie_%s_Body" % role
    modifier = body.modifiers.get("Haetae Rigid Rig")
    if modifier is not None:
        modifier.name = "Zombie Humanoid Rigid Rig"
    return body


def aim_at(obj, target):
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def render_scene(path, role=None, gallery=False):
    bpy.ops.mesh.primitive_plane_add(size=24, location=(0.0, 0.0, -1.02))
    floor = bpy.context.object
    floor.name = "Preview_Floor"
    floor.data.materials.append(geo.make_material(
        "MAT_PreviewFloor", (0.025, 0.035, 0.04), 0.08, 0.76))

    if gallery:
        camera_location = (5.8, -10.8, 3.3)
        target = (0.0, 0.0, 0.05)
        ortho_scale = 6.2
        resolution = (1600, 1000)
    else:
        camera_location = (3.4, -6.6, 2.2)
        target = (0.0, -0.05, 0.0)
        ortho_scale = 3.25
        resolution = (1100, 1200)

    bpy.ops.object.camera_add(location=camera_location)
    camera = bpy.context.object
    camera.name = "Preview_Camera"
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = ortho_scale
    aim_at(camera, target)
    bpy.context.scene.camera = camera

    lights = [
        ("Key", (3.8, -4.5, 5.8), 1250, 4.2, (1.0, 0.84, 0.7)),
        ("Fill", (-4.0, -2.2, 3.2), 900, 3.8, (0.42, 0.65, 1.0)),
        ("Rim", (0.0, 3.8, 4.2), 1450, 3.2, (1.0, 0.15, 0.08)),
    ]
    for name, location, energy, size, color in lights:
        bpy.ops.object.light_add(type="AREA", location=location)
        light = bpy.context.object
        light.name = "Preview_" + name
        light.data.energy = energy
        light.data.shape = "DISK"
        light.data.size = size
        light.data.color = color
        aim_at(light, target)

    world = bpy.context.scene.world
    world.color = (0.006, 0.009, 0.012)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.render.resolution_x = resolution[0]
    scene.render.resolution_y = resolution[1]
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.filepath = str(path)
    scene.render.film_transparent = False
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.view_settings.exposure = 0.5
    bpy.ops.render.render(write_still=True)


def build_role(role):
    clear_scene()
    create_collection("ZOMBIE_" + role.upper())
    materials = build_materials()
    pose = runner_pose() if role == "Runner" else (
        bruiser_pose() if role == "Bruiser" else ripper_pose())
    build_shared_anatomy(role, pose, materials)
    if role == "Runner":
        build_runner(pose, materials)
    elif role == "Bruiser":
        build_bruiser(pose, materials)
    else:
        build_ripper(pose, materials)
    armature = build_rig(role, pose)
    body = consolidate(role, armature)

    source_vertices = len(body.data.vertices)
    used_materials = {
        body.data.materials[polygon.material_index].name
        for polygon in body.data.polygons
        if polygon.material_index < len(body.data.materials)
    }
    if source_vertices <= 16000:
        raise RuntimeError("%s source has only %d vertices" % (role, source_vertices))
    if len(used_materials) != 5:
        raise RuntimeError("%s uses %d materials" % (role, len(used_materials)))

    lod0_path = MODEL_DIR / ("Zombie_%s_LOD0.fbx" % role)
    lod1_path = MODEL_DIR / ("Zombie_%s_LOD1.fbx" % role)
    preview_path = MODEL_DIR / ("Zombie_%s_Preview.png" % role)
    blend_path = SCRIPT_DIR / ("Zombie_%s.blend" % role)
    geo.export_fbx(lod0_path, armature)
    geo.export_fbx(lod1_path, armature, decimate_ratio=ROLES[role]["lod_ratio"])
    render_scene(preview_path, role=role)
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path), compress=True)
    print("ZOMBIE_SOURCE role=%s vertices=%d materials=%d" % (
        role, source_vertices, len(used_materials)))
    return {
        "role": role,
        "source_vertices": source_vertices,
        "lod0": lod0_path,
        "lod1": lod1_path,
        "preview": preview_path,
        "blend": blend_path,
    }


def inspect_fbx(path):
    clear_scene()
    bpy.ops.import_scene.fbx(filepath=str(path), use_anim=False)
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    armatures = [obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE"]
    vertex_count = sum(len(obj.data.vertices) for obj in meshes)
    material_usage = {}
    for obj in meshes:
        for polygon in obj.data.polygons:
            if polygon.material_index >= len(obj.data.materials):
                continue
            material = obj.data.materials[polygon.material_index]
            name = material.name if material is not None else ""
            material_usage[name] = material_usage.get(name, 0) + 1
    bone_names = {
        bone.name for armature in armatures for bone in armature.data.bones}
    return {
        "vertices": vertex_count,
        "materials": material_usage,
        "bones": bone_names,
    }


def add_gallery_label(text, x):
    bpy.ops.object.text_add(
        location=(x, -0.72, -1.0),
        rotation=(math.radians(90), 0.0, 0.0))
    label = bpy.context.object
    label.name = "Label_" + text
    label.data.body = text.upper()
    label.data.align_x = "CENTER"
    label.data.align_y = "CENTER"
    label.data.size = 0.22
    label.data.extrude = 0.008
    label.data.bevel_depth = 0.004
    label.data.materials.append(geo.make_material(
        "MAT_Label_" + text, (0.54, 0.62, 0.59), 0.1, 0.45))


def build_gallery(results):
    clear_scene()
    create_collection("ZOMBIE_GALLERY")
    offsets = (-2.15, 0.0, 2.15)
    for result, offset in zip(results, offsets):
        before = set(bpy.context.scene.objects)
        bpy.ops.import_scene.fbx(filepath=str(result["lod0"]), use_anim=False)
        imported = [obj for obj in bpy.context.scene.objects if obj not in before]
        root = bpy.data.objects.new("Gallery_%s" % result["role"], None)
        bpy.context.collection.objects.link(root)
        root.location.x = offset
        for obj in imported:
            if obj.parent is None:
                world = obj.matrix_world.copy()
                obj.parent = root
                obj.matrix_world = world
        add_gallery_label(result["role"], offset)
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=64, radius=0.72 if result["role"] != "Bruiser" else 0.88,
            depth=0.06, location=(offset, 0.0, -1.0))
        plinth = bpy.context.object
        plinth.name = "Plinth_" + result["role"]
        plinth.data.materials.append(geo.make_material(
            "MAT_Plinth_" + result["role"],
            (0.07, 0.085, 0.085), 0.65, 0.3))
    render_scene(MODEL_DIR / "Zombie_Models_Gallery.png", gallery=True)


def main():
    MODEL_DIR.mkdir(parents=True, exist_ok=True)
    SCRIPT_DIR.mkdir(parents=True, exist_ok=True)
    results = [build_role(role) for role in ("Runner", "Bruiser", "Ripper")]

    for result in results:
        lod0 = inspect_fbx(result["lod0"])
        lod1 = inspect_fbx(result["lod1"])
        if lod0["vertices"] <= 16000:
            raise RuntimeError("%s LOD0 vertex threshold failed" % result["role"])
        if lod1["vertices"] <= 500 or lod1["vertices"] >= lod0["vertices"] * 0.7:
            raise RuntimeError("%s LOD1 ratio failed" % result["role"])
        if len([count for count in lod0["materials"].values() if count > 0]) != 5:
            raise RuntimeError("%s LOD0 material population failed" % result["role"])
        if not set(REQUIRED_BONES).issubset(lod0["bones"]):
            missing = sorted(set(REQUIRED_BONES) - lod0["bones"])
            raise RuntimeError("%s rig missing %s" % (result["role"], missing))
        result["lod0_vertices"] = lod0["vertices"]
        result["lod1_vertices"] = lod1["vertices"]
        print(
            "ZOMBIE_FBX role=%s lod0=%d lod1=%d ratio=%.3f materials=%d bones=%d"
            % (result["role"], lod0["vertices"], lod1["vertices"],
               lod1["vertices"] / lod0["vertices"],
               len(lod0["materials"]), len(lod0["bones"])))

    build_gallery(results)
    print("ZOMBIE_MODELS_BUILD_COMPLETE")
    for result in results:
        print("OUTPUT role=%s blend=%s preview=%s" % (
            result["role"], result["blend"], result["preview"]))
    print("GALLERY=" + str(MODEL_DIR / "Zombie_Models_Gallery.png"))


if __name__ == "__main__":
    main()
