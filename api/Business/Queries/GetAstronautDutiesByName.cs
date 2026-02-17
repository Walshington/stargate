using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Domain;
using StargateAPI.Domain.Dtos;
using StargateAPI.Domain.Exceptions;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            const string personQuery = "SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE LOWER(a.Name) = LOWER(@name)";
            var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronautDto>(personQuery, new { name = request.Name });
            if (person is null)
                throw new NotFoundException("Person not found");

            const string dutiesQuery = "SELECT * FROM [AstronautDuty] WHERE @personId = PersonId Order By DutyStartDate Desc";
            var duties = await _context.Connection.QueryAsync<AstronautDuty>(dutiesQuery, new { personId = person.PersonId });

            return new GetAstronautDutiesByNameResult { Person = person, AstronautDuties = [.. duties] };
        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public required PersonAstronautDto Person { get; set; }
        public required List<AstronautDuty> AstronautDuties { get; set; }
    }
}
