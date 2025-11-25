using ComplaintManagement.Application.Common.Models;
using MediatR;

namespace ComplaintManagement.Application.Features.Categories.Commands;

public class DeleteCategoryCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }

    public DeleteCategoryCommand(Guid id)
    {
        Id = id;
    }
}
