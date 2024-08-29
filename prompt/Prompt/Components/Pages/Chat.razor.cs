namespace Prompt.Components.Pages
{
    public partial class Chat
    {
        public string? chat;
        protected override void OnInitialized()
        {
            base.OnInitialized();
            chat = "Digite";
        }

        public void onSubmit()
        {
            Console.WriteLine(chat);
        }
    }
}
