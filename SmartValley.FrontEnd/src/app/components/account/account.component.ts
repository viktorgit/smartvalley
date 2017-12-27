import {Component, OnInit} from '@angular/core';
import {BalanceService} from '../../services/balance/balance.service';
import {Balance} from '../../services/balance/balance';
import {AuthenticationService} from '../../services/authentication/authentication-service';
import {BlockiesService} from '../../services/blockies-service';
import {Paths} from '../../paths';
import {Router} from '@angular/router';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css']
})
export class AccountComponent implements OnInit {

  public currentBalance: number;
  public currentTokens: number;
  public accountAddress: string;
  public accountImgUrl: string;

  constructor(private router: Router,
              private balanceService: BalanceService,
              private blockiesService: BlockiesService,
              private authenticationService: AuthenticationService) {

    this.balanceService.balanceChanged.subscribe((balance: Balance) => this.updateBalances(balance));
    this.authenticationService.accountChanged.subscribe((user) => {
      if (user) {
        this.accountAddress = user.account;
        this.accountImgUrl = this.blockiesService.getImageForAddress(user.account);
      } else {
        this.router.navigate([Paths.Root]);
      }
    });
  }

  async ngOnInit() {
    await this.balanceService.updateBalanceAsync();
    const currentUser = this.authenticationService.getCurrentUser();
    if (currentUser) {
      this.accountAddress = currentUser.account;
      this.accountImgUrl = this.blockiesService.getImageForAddress(currentUser.account);
    } else {
      await this.router.navigate([Paths.Root]);
    }
  }

  private updateBalances(balance: Balance): void {
    if (balance != null) {
      this.currentBalance = balance.ethBalance;
      this.currentTokens = balance.svtBalance;
    }
  }

}
