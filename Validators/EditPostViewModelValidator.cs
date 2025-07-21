using BlogMvc.Models.ViewModels;
using FluentValidation;

namespace BlogMvc.Validators;

public class EditPostViewModelValidator : AbstractValidator<EditPostViewModel>
{
    public EditPostViewModelValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("არასწორი პოსტის ID");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("სათაური სავალდებულოა")
            .Length(1, 200).WithMessage("სათაური უნდა იყოს 1-200 სიმბოლოს შორის")
            .Must(BeValidTitle).WithMessage("სათაური არ უნდა შეიცავდეს მხოლოდ ცარიელ სიმბოლოებს");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("შინაარსი სავალდებულოა")
            .Length(10, 5000).WithMessage("შინაარსი უნდა იყოს 10-5000 სიმბოლოს შორის")
            .Must(BeValidContent).WithMessage("შინაარსი არ უნდა შეიცავდეს მხოლოდ ცარიელ სიმბოლოებს");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("ავტორი სავალდებულოა")
            .Length(1, 100).WithMessage("ავტორის სახელი უნდა იყოს 1-100 სიმბოლოს შორის")
            .Must(BeValidAuthor).WithMessage("ავტორის სახელი არ უნდა შეიცავდეს მხოლოდ ცარიელ სიმბოლოებს");
    }

    private bool BeValidTitle(string title)
    {
        return !string.IsNullOrWhiteSpace(title);
    }

    private bool BeValidContent(string content)
    {
        return !string.IsNullOrWhiteSpace(content);
    }

    private bool BeValidAuthor(string author)
    {
        return !string.IsNullOrWhiteSpace(author);
    }
}