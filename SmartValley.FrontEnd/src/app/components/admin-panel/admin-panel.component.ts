import {Component, OnInit} from '@angular/core';
import {AdminApiClient} from '../../api/admin/admin-api-client';
import {DialogService} from '../../services/dialog-service';
import {AdminContractClient} from '../../services/contract-clients/admin-contract-client';
import {AuthenticationService} from '../../services/authentication/authentication-service';
import {Router} from '@angular/router';
import {AdminResponse} from '../../api/admin/admin-response';
import {NotificationsService} from 'angular2-notifications';
import {UserApiClient} from '../../api/user/user-api-client';
import {ErrorCode} from '../../shared/error-code.enum';
import {TranslateService} from '@ngx-translate/core';
import {UserContext} from '../../services/authentication/user-context';

@Component({
  selector: 'app-admin-panel',
  templateUrl: './admin-panel.component.html',
  styleUrls: ['./admin-panel.component.css']
})
export class AdminPanelComponent implements OnInit {

  public admins: Array<AdminResponse> = [];

  constructor(private router: Router,
              private adminApiClient: AdminApiClient,
              private adminContractClient: AdminContractClient,
              private authenticationService: AuthenticationService,
              private notificationsService: NotificationsService,
              private userApiClient: UserApiClient,
              private dialogService: DialogService,
              private userContext: UserContext,
              private translateService: TranslateService) {
  }

  async ngOnInit(): Promise<void> {
    await this.updateAdminsAsync();
  }

  async createAsync() {
    try {
      const address = await this.dialogService.showCreateAdminDialogAsync()
      const user = await this.userApiClient.getByAddressAsync(address);
      const transactionHash = await this.adminContractClient.addAsync(address);

      await this.adminApiClient.addAsync(address, transactionHash);
    } catch (e) {
      if (e.error.errorCode === ErrorCode.UserNotFound) {
        this.notificationsService.error(this.translateService.instant('Common.UserNotFound'));
      }
    }
    await this.updateAdminsAsync();
  }

  async deleteAsync(address: string) {
    const fromAddress = await this.userContext.getCurrentUser().account;
    const transactionHash = await this.adminContractClient.deleteAsync(address, fromAddress);
    await this.adminApiClient.deleteAsync(address, transactionHash);
    await this.updateAdminsAsync();
  }

  private async updateAdminsAsync() {
    const response = await this.adminApiClient.getAllAsync();
    this.admins = response.items;
  }
}