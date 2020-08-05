using System;
using System.Collections.Generic;
using System.Text;
using Collada141;
using OpenTK;
using System.Linq;

namespace Toolbox.Core.Collada
{
    public class DaeUtility
    {
        public static controller FindControllerFromNode(instance_controller instance_controller, library_controllers controllers)
        {
            string mesh_id = instance_controller.url.Trim('#');
            return Array.Find(controllers.controller, x => x.id == mesh_id);
        }

        public static geometry FindGeoemertyFromController(controller controller, library_geometries geometries)
        {
            skin skin = controller.Item as skin;
            string mesh_id = skin.source1.Trim('#');
            return Array.Find(geometries.geometry, x => x.id == mesh_id);
        }

        public static geometry FindGeoemertyFromNode(node daeNode, library_geometries geometries)
        {
            string mesh_id = daeNode.instance_geometry[0].url.Trim('#');
            return Array.Find(geometries.geometry, x => x.id == mesh_id);
        }

        public static source FindSourceFromInput(InputLocalOffset input, source[] sources)
        {
            string inputSource = input.source.Trim('#');
            if (sources.Any(x => x.id == inputSource))
                return Array.Find(sources, x => x.id == inputSource);
            else
                return null;
        }

        public static source FindSourceFromInput(InputLocal input, source[] sources)
        {
            string inputSource = input.source.Trim('#');
            if (sources.Any(x => x.id == inputSource))
                return Array.Find(sources, x => x.id == inputSource);
            else
                return null;
        }

        public static Matrix4 GetMatrix(object[] items)
        {
            Matrix4 transform = Matrix4.Identity;
            for (int i = 0; i < items?.Length; i++)
            {
                if (items[i] is matrix)
                    transform = FloatToMatrix(((matrix)items[i]).Values);
            }
            return transform;
        }

        public static Matrix4 GetLocalTransform(node node)
        {
            Matrix4 transform = Matrix4.Identity;
            Matrix4 translate = Matrix4.Identity;
            Matrix4 rotateX = Matrix4.Identity;
            Matrix4 rotateY = Matrix4.Identity;
            Matrix4 rotateZ = Matrix4.Identity;
            Matrix4 scale = Matrix4.Identity;

            for (int i = 0; i < node.ItemsElementName?.Length; i++)
            {
                switch (node.ItemsElementName[i])
                {
                    case ItemsChoiceType2.matrix:
                        transform = FloatToMatrix(((matrix)node.Items[i]).Values);
                        break;
                    case ItemsChoiceType2.translate:
                        translate = TranslationToMatrix4(((TargetableFloat3)node.Items[i]));
                        break;
                    case ItemsChoiceType2.rotate:
                        string sid = ((rotate)node.Items[i]).sid;
                        if (sid == "rotateX")
                            rotateX = RotationToMatrix4(((rotate)node.Items[i]));
                        if (sid == "rotateY")
                            rotateY = RotationToMatrix4(((rotate)node.Items[i]));
                        if (sid == "rotateZ")
                            rotateZ = RotationToMatrix4(((rotate)node.Items[i]));
                        break;
                    case ItemsChoiceType2.scale:
                        scale = ScaleToMatrix4(((TargetableFloat3)node.Items[i]));
                        break;
                }
            }
            return (scale * (rotateX * rotateY * rotateZ) * translate) * transform;
        }

        public static Matrix4 RotationToMatrix4(rotate r)
        {
            var axis = new Vector3((float)r.Values[0], (float)r.Values[1], (float)r.Values[2]);
            var rot = Quaternion.FromAxisAngle(axis, (float)MathHelper.DegreesToRadians(r.Values[3]));
            return Matrix4.CreateFromQuaternion(rot);
        }

        public static Matrix4 TranslationToMatrix4(TargetableFloat3 t) {
            return Matrix4.CreateTranslation((float)t.Values[0], (float)t.Values[1], (float)t.Values[2]);
        }

        public static Matrix4 ScaleToMatrix4(TargetableFloat3 t) {
            return Matrix4.CreateScale((float)t.Values[0], (float)t.Values[1], (float)t.Values[2]);
        }

        public static Matrix4 FloatToMatrix(float[] values)
        {
            if (values?.Length != 16)
                return Matrix4.Identity;

            var mat = new Matrix4(
               values[0], values[1], values[2], values[3],
               values[4], values[5], values[6], values[7],
               values[8], values[9], values[10], values[11],
               values[12], values[13], values[14], values[15]);

            mat.Transpose();
            return mat;
        }

        public static Matrix4 FloatToMatrix(double[] values)
        {
            if (values?.Length != 16)
                return Matrix4.Identity;

            var mat = new Matrix4(
               (float)values[0], (float)values[1], (float)values[2], (float)values[3],
               (float)values[4], (float)values[5], (float)values[6], (float)values[7],
               (float)values[8], (float)values[9], (float)values[10], (float)values[11],
               (float)values[12], (float)values[13], (float)values[14], (float)values[15]);

            mat.Transpose();
            return mat;
        }

        public static int FindMaterialIndex(library_materials materials, string name)
        {
            if (materials != null)
            {
                for (int i = 0; i < materials.material.Length; i++)
                {
                    if (materials.material[i].id == name ||
                        materials.material[i].name == name)
                        return i;
                }
            }
            return -1;
        }
    }
}
