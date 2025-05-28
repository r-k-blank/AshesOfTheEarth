namespace AshesOfTheEarth.Entities.Visitor
{
    public interface IEntityVisitor
    {
        void VisitPlayer(Entity playerEntity);
        void VisitResourceNode(Entity resourceEntity);
        void VisitGenericEntity(Entity genericEntity);
    }
}