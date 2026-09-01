import { RoleGuard } from './role-guard';

describe('RoleGuard', () => {
  it('should create an instance', () => {
    const directive = new RoleGuard();
    expect(directive).toBeTruthy();
  });
});
