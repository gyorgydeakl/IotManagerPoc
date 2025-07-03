using FastEndpoints;
using IotManagerApi.Database;
using IotManagerApi.Dto;
using IotManagerApi.Services;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.EntityFrameworkCore;

namespace IotManagerApi.Endpoints;

public class CreateBatchJobEndpoint(IotManagerDbContext dbContext, TimeProvider timeProvider) : Endpoint<CreateBatchJobRequest, BatchJobDto>
{
    public override void Configure()
    {
        Post("/batch-jobs");
        AllowAnonymous();
        Description(x => x.WithName("CreateBatchJob")
            .WithSummary("Creates a new batch job")
            .WithOpenApi());
    }

    public override async Task<BatchJobDto> ExecuteAsync(CreateBatchJobRequest req, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().DateTime;
        var batchJob = new BatchJob
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Description = req.Description,
            TagsToSet = req.TagsToSet,
            TagsToDelete = req.TagsToDelete.Select(tag => new TagKey(tag)).ToList(),
            PropertiesToSet = req.PropertiesToSet,
            PropertiesToDelete = req.PropertiesToDelete.Select(p => new PropertyKey(p)).ToList(),
            CreatedAt = now,
            UpdatedAt = now,
            DeviceIds = req.DeviceIds.Select(id => new DeviceId(id)).ToList(),
        };

        dbContext.DeviceGroups.Add(batchJob);
        await dbContext.SaveChangesAsync(ct);

        return batchJob.ToBatchJobDto();
    }
}

public class ListBatchJobsEndpoint(IotManagerDbContext dbContext) : Endpoint<EmptyRequest, IEnumerable<BatchJobDto>>
{
    public override void Configure()
    {
        Get("/batch-jobs");
        AllowAnonymous();
        Description(x => x.WithName("ListBatchJobs")
            .WithSummary("Retrieves a list of all batch jobs")
            .WithOpenApi());
    }

    public override async Task<IEnumerable<BatchJobDto>> ExecuteAsync(EmptyRequest req, CancellationToken ct)
    {
        var batchJobs = await dbContext.DeviceGroups.ToListAsync(ct);
        return batchJobs.Select(bj => bj.ToBatchJobDto());
    }
}

public class GetBatchJobByIdEndpoint(IotManagerDbContext dbContext) : Endpoint<GetBatchJobRequest, IResult>
{
    public override void Configure()
    {
        Get("/batch-jobs/{JobId}");
        AllowAnonymous();
        Description(x => x.WithName("GetBatchJobById")
            .WithSummary("Retrieves a batch job by its ID")
            .WithOpenApi());
    }

    public override async Task<IResult> ExecuteAsync(GetBatchJobRequest req, CancellationToken ct)
    {
        var batchJob = await dbContext.DeviceGroups.FindAsync([req.JobId], ct);
        return batchJob == null ? Results.NotFound() : Results.Ok(batchJob.ToBatchJobDto());
    }
}

public class ExecuteBatchJobEndpoint(IotManagerDbContext dbContext, BatchJobService batchJobService, TimeProvider timeProvider) : Endpoint<ExecuteBatchJobRequest, IResult>
{
    public override void Configure()
    {
        Post("/batch-jobs/{JobId}/execute");
        AllowAnonymous();
        Description(x => x.WithName("ExecuteBatchJob")
            .WithSummary("Executes a batch job to update device tags")
            .WithOpenApi());
    }

    public override async Task<IResult> ExecuteAsync(ExecuteBatchJobRequest req, CancellationToken ct)
    {
        var batchJob = await dbContext.DeviceGroups.FindAsync([req.JobId], ct);
        if (batchJob == null)
        {
            return Results.NotFound();
        }
        if (batchJob.DeviceIds.Count == 0)
        {
            return Results.BadRequest("No devices specified for the batch job.");
        }

        var jobResponse = await batchJobService.ExecuteBatchJobAsync(batchJob.ToSingleTimeExecute(), ct);
        if (jobResponse.Status == JobStatus.Failed)
        {
            return Results.Problem("Job execution failed. See the error details in the response body.", jobResponse.FailureReason, statusCode: 400);
        }

        batchJob.UpdatedAt = timeProvider.GetUtcNow().DateTime;
        dbContext.DeviceGroups.Update(batchJob);

        await dbContext.SaveChangesAsync(ct);
        return Results.Ok(batchJob.ToBatchJobDto());
    }
}

public class ExecuteSingleTimeBatchJobEndpoint(BatchJobService batchJobService) : Endpoint<ExecuteSingleTimeBatchJobRequest, IResult>
{
    public override void Configure()
    {
        Post("/batch-jobs/single-time/execute");
        AllowAnonymous();
        Description(x => x.WithName("ExecuteSingleTimeBatchJob")
            .WithSummary("Executes a single-time batch job to update device tags")
            .WithOpenApi());
    }

    public override async Task<IResult> ExecuteAsync(ExecuteSingleTimeBatchJobRequest req, CancellationToken ct)
    {
        var jobResponse = await batchJobService.ExecuteBatchJobAsync(req, ct);
        return jobResponse.Status == JobStatus.Failed ?
            Results.Problem("Job execution failed. See the error details in the response body.", jobResponse.FailureReason, statusCode: 400) :
            Results.Ok();
    }
}