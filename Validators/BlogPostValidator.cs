using BlogMvc.Models;
using FluentValidation;

namespace BlogMvc.Validators;
public class BlogPostValidator : AbstractValidator<BlogPost>
{
    public BlogPostValidator()
    {
        RuleFor(x => x.Title)
              .NotEmpty().WithMessage("სათაური სავალდებულოა")
              .Length(1, 200).WithMessage("სათაური უნდა იყოს 1-200 სიმბოლოს შორის");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("შინაარსი სავალდებულოა")
            .Length(10, 5000).WithMessage("შინაარსი უნდა იყოს 10-5000 სიმბოლოს შორის");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("ავტორი სავალდებულოა")
            .Length(1, 100).WithMessage("ავტორის სახელი უნდა იყოს 1-100 სიმბოლოს შორის");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("მომხმარებელი სავალდებულოა");
    }
}
