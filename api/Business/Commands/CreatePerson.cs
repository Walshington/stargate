using System.Net;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Domain;
using StargateAPI.Domain.Dtos;
using StargateAPI.Domain.Exceptions;

namespace StargateAPI.Business.Commands
{
    public class CreatePerson : IRequest<CreatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
    {
        private readonly StargateContext _context;
        public CreatePersonPreProcessor(StargateContext context)
        {
            _context = context;
        }
        public Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new UnprocessableEntityException("Person name is required and cannot be empty.");

            if (request.Name.Trim().Length < 2)
                throw new UnprocessableEntityException("Person name must be at least 2 characters.");

            if (request.Name.Length > 100)
                throw new UnprocessableEntityException("Person name must be at most 100 characters.");

            if (_context.People.AsNoTracking().Any(z => z.Name == request.Name))
                throw new UnprocessableEntityException("A person with this name already exists.");

            return Task.CompletedTask;
        }
    }

    public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
    {
        private readonly StargateContext _context;

        public CreatePersonHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {

            var newPerson = new Person()
            {
                Name = request.Name
            };

            await _context.People.AddAsync(newPerson);

            await _context.SaveChangesAsync();

            return new CreatePersonResult()
            {
                ResponseCode = (int)HttpStatusCode.Created,
                Person = new PersonAstronaut
                {
                    PersonId = newPerson.Id,
                    Name = newPerson.Name
                }
            };

        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public required PersonAstronaut Person { get; set; }
    }
}
