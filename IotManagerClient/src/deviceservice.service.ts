import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private apiUrl = 'https://localhost:44398/devices';

  constructor(private http: HttpClient) { }

  getDevices(): Observable<RawDevice[]> {
    return this.http.get<RawDevice[]>(this.apiUrl);
  }

  createDevice(deviceId: string): Observable<RawDevice> {
    return this.http.post<RawDevice>(this.apiUrl, { deviceId });
  }

  updateDeviceTwin(deviceId: string, twin: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${deviceId}/twin`, twin);
  }

  executeCommand(deviceIds: string[], command: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/executeCommand`, { deviceIds, command });
  }
}