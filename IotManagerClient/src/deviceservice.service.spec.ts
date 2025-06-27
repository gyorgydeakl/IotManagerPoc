import { TestBed } from '@angular/core/testing';

import { DeviceserviceService } from './deviceservice.service';

describe('DeviceserviceService', () => {
  let service: DeviceserviceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DeviceserviceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
