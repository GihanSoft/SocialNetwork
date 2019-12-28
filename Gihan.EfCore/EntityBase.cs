using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.EfCore
{
    public interface IEntityBase<TId>
        where TId : struct
    {
        TId Id { get; set; }
    }
    public interface IEntityBase : IEntityBase<long>
    {
    }

    public abstract class EntityBase<TId> : IEntityBase<TId>
        where TId : struct
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TId Id { get; set; }
    }

    public abstract class EntityBase : EntityBase<long>, IEntityBase
    {
    }
}
