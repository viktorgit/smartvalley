import {Component, OnInit} from '@angular/core';
import {AuthenticationService} from '../../services/authentication-service';
import {Web3Service} from '../../services/web3-service';
import {User} from '../../services/user';
import {Router} from '@angular/router';
import {Paths} from '../../paths';
import {NotificationsService} from 'angular2-notifications';

@Component({
  selector: 'app-root',
  templateUrl: './root.component.html',
  styleUrls: ['./root.component.css']
})
export class RootComponent implements OnInit {

  public userInfo: User;

  constructor(private web3Service: Web3Service,
              private authenticationService: AuthenticationService,
              private notificationsService: NotificationsService,
              private router: Router) {
    // this.authenticationService.userInfoChanged.subscribe(async () => await this.updateUserInfo());
  }

  async ngOnInit() {
  //  await this.updateUserInfo();
  }

  async updateUserInfo() {
  //  this.userInfo = await this.authenticationService.getUser();
  }

  async navigateToScoring() {
    await this.router.navigate([Paths.Scoring]);
  }

  async createProject() {
    const isOk = await this.authenticationService.authenticate();
    if (isOk) {
      await this.router.navigate([Paths.Application]);
    }
  }
}
