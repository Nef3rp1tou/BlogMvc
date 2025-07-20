using FluentValidation;

namespace BlogMvc.Validators;
public class SearchViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
}

public class SearchViewModelValidator : AbstractValidator<SearchViewModel>
{
    public SearchViewModelValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("საძიებო ტერმინი არ უნდა იყოს 100 სიმბოლოზე მეტი")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}