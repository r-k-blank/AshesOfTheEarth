namespace AshesOfTheEarth.Entities.Visitor
{
    public interface IEntityElement
    {
        void Accept(IEntityVisitor visitor);
    }
}