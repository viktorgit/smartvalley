import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot} from '@angular/router';
import {BalanceService} from './balance.service';

@Injectable()
export class ShouldHaveEthGuard implements CanActivate {
  constructor(private balanceService: BalanceService) {
  }

  public canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    return this.balanceService.checkEthAsync();
  }
}
