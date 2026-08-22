import { TestBed } from '@angular/core/testing';

import { LeaveType } from './leave-type';

describe('LeaveType', () => {
  let service: LeaveType;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LeaveType);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
