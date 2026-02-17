using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Domain;
using StargateAPI.Domain.Dtos;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        public readonly StargateContext _context;
        public GetPeopleHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            const string query = "SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id";
            var people = await _context.Connection.QueryAsync<PersonAstronautDto>(query);

            return new GetPeopleResult { People = [.. people] };
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public required List<PersonAstronautDto> People { get; set; }

    }
}
