import { TestBed } from '@angular/core/testing';

import { TimesheetConfig } from './timesheet-config';

describe('TimesheetConfig', () => {
  let service: TimesheetConfig;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimesheetConfig);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
