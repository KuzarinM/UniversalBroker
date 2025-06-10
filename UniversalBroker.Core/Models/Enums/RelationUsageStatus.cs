namespace UniversalBroker.Core.Models.Enums
{
    /// <summary>
    /// Статус использования связи
    /// </summary>
    public enum RelationUsageStatus
    {
        /// <summary>
        /// Используется
        /// </summary>
        InUse = 0,

        /// <summary>
        /// Не используется, но объявлено
        /// </summary>
        NotUsed = 1,

        /// <summary>
        /// Используется, но не отмечено
        /// </summary>
        NotMarked = 2
    }
}
