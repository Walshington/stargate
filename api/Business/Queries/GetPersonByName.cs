using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Domain;
using StargateAPI.Domain.Dtos;
using StargateAPI.Domain.Exceptions;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;
        public GetPersonByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            const string query = "SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE LOWER(a.Name) = LOWER(@name)";
            var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronautDto>(query, new { name = request.Name });
            if (person is null)
                throw new NotFoundException("Person not found");

            return new GetPersonByNameResult { Person = person };
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public required PersonAstronautDto Person { get; set; }
    }
}
