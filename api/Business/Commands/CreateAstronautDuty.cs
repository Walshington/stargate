using System.Net;
using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Domain;
using StargateAPI.Domain.Dtos;
using StargateAPI.Domain.Exceptions;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }

        /// <summary>Set by preprocessor after resolving person by name; handler uses this to avoid a second lookup.</summary>
        public int PersonId { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        /* Verify person exists and no previous duty exists for the same duty title and start date. */
        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name.ToLower() == request.Name.ToLower());

            if (person is null)
                throw new NotFoundException("Person not found");

            request.PersonId = person.Id;

            var verifyNoPreviousDuty = _context.AstronautDuties.FirstOrDefault(z =>
                z.DutyTitle == request.DutyTitle && z.DutyStartDate.Date == request.DutyStartDate.Date);

            if (verifyNoPreviousDuty is not null) 
                throw new ConflictException("A duty with the same title and start date already exists.");

            return Task.CompletedTask;
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            /* PersonId resolved by preprocessor */
            var personId = request.PersonId;

            /* Query to get astronaut detail */
            const string astronautDetailQuery = "SELECT * FROM [AstronautDetail] WHERE @personId = PersonId";
            var astronautDetail = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDetail>(astronautDetailQuery, new { personId });

            /* 
            If astronaut detail does not exist, create new astronaut detail.
            If it does, update the existing astronaut detail.
            */
            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail();
                astronautDetail.PersonId = personId;
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                astronautDetail.CareerStartDate = request.DutyStartDate.Date;
                if (request.DutyTitle == "RETIRED")
                {
                    astronautDetail.CareerEndDate = request.DutyStartDate.Date;
                }

                await _context.AstronautDetails.AddAsync(astronautDetail);

            }
            else
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                /* If duty title is "RETIRED", set career end date to duty start date - 1 day. */
                if (request.DutyTitle == "RETIRED")
                {
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }
                _context.AstronautDetails.Update(astronautDetail);
            }

            /* Query to get astronaut duty and update the previous duty end date if it exists. */
            const string astronautDutyQuery = "SELECT * FROM [AstronautDuty] WHERE @personId = PersonId Order By DutyStartDate Desc";
            var astronautDuty = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(astronautDutyQuery, new { personId });

            if (astronautDuty != null)
            {
                astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                _context.AstronautDuties.Update(astronautDuty);
            }

            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = personId,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = request.DutyStartDate.Date,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty);

            await _context.SaveChangesAsync();

            /* Return the new astronaut duty*/
            return new CreateAstronautDutyResult()
            {
                ResponseCode = (int)HttpStatusCode.Created,
                AstronautDuty = new AstronautDutyDto
                {
                    Id = newAstronautDuty.Id,
                    PersonId = newAstronautDuty.PersonId,
                    Rank = newAstronautDuty.Rank,
                    DutyTitle = newAstronautDuty.DutyTitle,
                    DutyStartDate = newAstronautDuty.DutyStartDate,
                    DutyEndDate = newAstronautDuty.DutyEndDate
                }
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public required AstronautDutyDto AstronautDuty { get; set; }
    }
}
