namespace FishingBuddy.Models.Ai;

public class AiFishDraftPageViewModel
{
    public string Prompt { get; set; } = string.Empty;
    public bool HasResult { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public AiFishDraftResultViewModel? Result { get; set; }
}
