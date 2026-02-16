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

            var result = new GetAstronautDutiesByNameResult();

            const string personQuery = "SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE LOWER(a.Name) = LOWER(@name)";
            var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(personQuery, new { name = request.Name });

            if (person is null)
                throw new NotFoundException("Person not found");

            result.Person = person;

            const string dutiesQuery = "SELECT * FROM [AstronautDuty] WHERE @personId = PersonId Order By DutyStartDate Desc";
            var duties = await _context.Connection.QueryAsync<AstronautDuty>(dutiesQuery, new { personId = person.PersonId });

            result.AstronautDuties = duties.ToList();

            return result;

        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
