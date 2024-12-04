namespace Mounters
{
    public interface IMounter
    {
        public void Mount(string name);

        public void Dismount();
    }
}
