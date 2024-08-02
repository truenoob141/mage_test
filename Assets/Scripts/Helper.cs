using MageTest.Core.Interfaces;
using UnityEngine;

namespace MageTest
{
    public static class Helper
    {
        public static bool IsValid(IEntity entity)
        {
            return entity != null && entity.IsValid;
        }

        public static Quaternion LookAt2D(Vector2 dir)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle - 90);
        }
        
        public static void LookAt2D(Transform transform, Vector2 dir)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);
        }
    }
}