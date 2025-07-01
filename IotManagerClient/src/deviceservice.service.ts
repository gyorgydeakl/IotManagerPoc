import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DeviceGroup } from './device-list/device-list.component';

export interface RawDevice {
  deviceId: string;
  modelId: string;
  connectionState: number;
  status: number;
  statusReason: string | null;
  lastActivityTime: string;
  capabilities: { iotEdge: boolean };
  tags: string;
  properties: { desired: string; reported: string };
  twin: any;
}

export interface TagKeyValuePair {
  /** nullable per schema */
  key: string ;
  value: string ;
}

export interface PropertyKeyValuePair {
  key: string;
  /** JSON tree node – pass whatever object you built in the editor */
  value: any;
}

export interface TagKey {
  /** nullable per schema */
  value: string ;
}

export interface PropertyKey {
  value: string ;
}

export interface CreateBatchJobRequest {
  name: string ;
  description: string ;
  tagsToSet: TagKeyValuePair[] ;
  tagsToDelete: string[];
  propertiesToSet: PropertyKeyValuePair[];
  propertiesToDelete: string[];
  deviceIds: string[];
}

export interface ExecuteBatchJobRequest {
  jobId: string;
}

export interface ExecuteSingleTimeBatchJobRequest {
  deviceIds?: string[];
  tagsToSet?: TagKeyValuePair[];
  tagsToDelete?: string[];
}

export interface BatchJobDto {
  id: string;
  name: string | null;
  description: string | null;
  deviceIds?: string[];
  tagsToSet?: { key: string; value: string }[];
  tagsToDelete?: string[];
  propertiesToSet?: PropertyKeyValuePair[];
  propertiesToDelete?: PropertyKey[];
}


@Injectable({ providedIn: 'root' })
export class DeviceService {
  private baseUrl = 'https://localhost:44398';
  private deviceUrl = `${this.baseUrl}/devices`;
  private batchUrl = `${this.baseUrl}/batch-jobs`;

  constructor(private http: HttpClient) { }

  getDevices(): Observable<RawDevice[]> {
    return this.http.get<RawDevice[]>(this.deviceUrl);
  }

  createDevice(deviceId: string): Observable<RawDevice> {
    return this.http.post<RawDevice>(this.deviceUrl, { deviceId });
  }

  createBatchJob(req: CreateBatchJobRequest): Observable<BatchJobDto> {
    return this.http.post<BatchJobDto>(this.batchUrl, req);
  }

  executeBatchJob(jobId: string): Observable<any> {
    return this.http.post(`${this.batchUrl}/${jobId}/execute`, {});
  }

  getDeviceGroups(): Observable<DeviceGroup[]> {
    return this.http.get<DeviceGroup[]>(this.deviceUrl);
  }
  executeSingleTimeBatchJob(
    req: ExecuteSingleTimeBatchJobRequest
  ): Observable<any> {
    return this.http.post(
      `${this.batchUrl}/single-time/execute`,
      req
    );
  }

   getBatchJobsList(): Observable<BatchJobDto[]> {
    return this.http.get<BatchJobDto[]>(this.batchUrl);
  }
}