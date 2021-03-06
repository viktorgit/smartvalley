import {Component, OnInit} from '@angular/core';
import {AuthenticationApiClient} from '../../../api/authentication/authentication-api-client';
import {ActivatedRoute, Router} from '@angular/router';
import {ConfirmEmailRequest} from '../../../api/authentication/confirm-email-request';
import {Paths} from '../../../paths';
import {NotificationsService} from 'angular2-notifications';
import {TranslateService} from '@ngx-translate/core';
import {AuthenticationService} from '../../../services/authentication/authentication-service';
import {DialogService} from '../../../services/dialog-service';
import {isNullOrUndefined} from 'util';

@Component({
  selector: 'app-confirm-email',
  templateUrl: './confirm-email.component.html',
  styleUrls: ['./confirm-email.component.css']
})
export class ConfirmEmailComponent implements OnInit {

  constructor(private dialogService: DialogService,
              private authenticationApiClient: AuthenticationApiClient,
              private router: Router,
              private activatedRoute: ActivatedRoute,
              private notificationService: NotificationsService,
              private translateService: TranslateService,
              private authenticationService: AuthenticationService) {
  }

  public async ngOnInit() {
    const token = this.activatedRoute.snapshot.params.token;
    const isChangeEmail = this.activatedRoute.snapshot.queryParams.changeEmail;
    try {
      await this.authenticationApiClient.confirmEmailAsync(<ConfirmEmailRequest>{
        token: token
      });
      this.notificationService.success(
        this.translateService.instant('ConfirmEmail.Success'),
        this.translateService.instant('ConfirmEmail.Confirmed'));
      await this.authenticationService.authenticateAsync();
      if (isNullOrUndefined(isChangeEmail)) {
        this.dialogService.showWelcome();
      }
    } catch (e) {
      this.notificationService.error(
        this.translateService.instant('ConfirmEmail.Error'),
        this.translateService.instant('ConfirmEmail.Failed'));
    }
    this.router.navigate([Paths.Root]);
  }
}
