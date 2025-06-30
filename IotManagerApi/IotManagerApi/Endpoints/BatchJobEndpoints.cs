using FastEndpoints;
using IotManagerApi.Database;
using IotManagerApi.Dto;
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
        Description(x => x.WithName("CreateBatchJob"));
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
        Description(x => x.WithName("ListBatchJobs"));
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
        Description(x => x.WithName("GetBatchJobById"));
    }

    public override async Task<IResult> ExecuteAsync(GetBatchJobRequest req, CancellationToken ct)
    {
        var batchJob = await dbContext.DeviceGroups.FindAsync([req.JobId], ct);
        return batchJob == null ? Results.NotFound() : Results.Ok(batchJob.ToBatchJobDto());
    }
}

public class ExecuteBatchJobEndpoint(IotManagerDbContext dbContext, JobClient jobClient, TimeProvider timeProvider) : Endpoint<ExecuteBatchJobRequest, IResult>
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

        var twinCollectionPatch = new TwinCollection();
        batchJob.TagsToSet.ForEach(t => twinCollectionPatch[t.Key] = t.Value);
        batchJob.TagsToDelete.ForEach(key => twinCollectionPatch[key.Value] = null);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        await jobClient.ScheduleTwinUpdateAsync(
            jobId: Guid.NewGuid().ToString(),
            queryCondition: "deviceId IN [" + string.Join(",", batchJob.DeviceIds.Select(d => $"'{d.Value}'")) + "]",
            twin: new Twin("*")
            {
                Tags = twinCollectionPatch,
                ETag = "*"
            },
            startTimeUtc: now.AddSeconds(1),
            maxExecutionTimeInSeconds: TimeSpan.FromMinutes(30).Seconds,
            cancellationToken: ct);

        batchJob.UpdatedAt = now;
        dbContext.DeviceGroups.Update(batchJob);

        await dbContext.SaveChangesAsync(ct);
        return Results.Ok(batchJob.ToBatchJobDto());
    }
}