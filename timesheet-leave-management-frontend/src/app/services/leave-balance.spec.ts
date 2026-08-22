import { TestBed } from '@angular/core/testing';

import { LeaveBalance } from './leave-balance';

describe('LeaveBalance', () => {
  let service: LeaveBalance;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LeaveBalance);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
