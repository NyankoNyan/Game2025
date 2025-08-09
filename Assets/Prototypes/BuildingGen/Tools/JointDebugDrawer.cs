using UnityEngine;

namespace BuildingGen.Tools
{
    /// <summary>
    /// Компонент для визуализации физических соединений (joints) в редакторе Unity и во время игры.
    /// Рисует линии между объектами, связанными через FixedJoint.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class JointDebugDrawer : MonoBehaviour
    {
        /// <summary>
        /// Флаг, определяющий, должны ли отрисовываться дебаг-линии.
        /// </summary>
        [SerializeField]
        private bool _drawGizmos = true;

        /// <summary>
        /// Отрисовка дебаг-линий для соединений.
        /// Вызывается каждый кадр в редакторе и во время игры.
        /// </summary>
        private void OnDrawGizmos()
        {
            // Прерываем выполнение, если отрисовка выключена
            if (!_drawGizmos)
                return;

            // Получаем актуальный список соединений каждый вызов
            FixedJoint[] joints = GetComponents<FixedJoint>();

            foreach (var joint in joints)
            {
                // Проверяем, что соединение и цель существуют
                if (joint == null || joint.connectedBody == null)
                    continue;

                // Рассчитываем цвет на основе силы
                float forceRatio = joint.currentForce.magnitude / joint.breakForce;
                Color gizmoColor = Color.Lerp(Color.green, Color.red, forceRatio);

                // Устанавливаем цвет
                Gizmos.color = gizmoColor;

                // Рисуем линию между текущим объектом и целью
                Gizmos.DrawLine(transform.position, joint.connectedBody.position);
            }
        }
    }
}
