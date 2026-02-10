namespace Nautilus.Interactables
{
    public abstract class InteractableBase
    {
        public InteractableBase()
        {
            InteractableInit.InteractableList.Add(this);
        }
        
        public abstract void Init();
    }
}