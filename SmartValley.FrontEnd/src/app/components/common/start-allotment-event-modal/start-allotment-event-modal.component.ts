import {Component, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material';
import {AddAdminModalComponent} from '../add-admin-modal/add-admin-modal.component';
import {ProjectApiClient} from '../../../api/project/project-api-client';
import {ProjectSummaryResponse} from '../../../api/project/project-summary-response';
import {NotificationsService} from 'angular2-notifications';
import {TranslateService} from '@ngx-translate/core';
import {AllotmentEventsManagerContractClient} from '../../../services/contract-clients/allotment-events-manager-contract-client';
import {AllotmentEventService} from '../../../services/allotment-event/allotment-event.service';
import {AllotmentEvent} from '../../../services/allotment-event/allotment-event';

@Component({
  selector: 'app-start-allotment-event-modal',
  templateUrl: './start-allotment-event-modal.component.html',
  styleUrls: ['./start-allotment-event-modal.component.scss']
})
export class StartAllotmentEventModalComponent implements OnInit {

  public project: ProjectSummaryResponse;
  public freezeTime: number;

  constructor(@Inject(MAT_DIALOG_DATA) public data: AllotmentEvent,
              private dialogRef: MatDialogRef<AddAdminModalComponent>,
              private allotmentEventsManagerContractClient: AllotmentEventsManagerContractClient,
              private notificationService: NotificationsService,
              private translateService: TranslateService,
              private projectApiClient: ProjectApiClient,
              private allotmentEventService: AllotmentEventService) {
  }

  async ngOnInit() {
    this.project = await this.projectApiClient.getProjectSummaryAsync(this.data.projectId);
    this.freezeTime = await this.allotmentEventsManagerContractClient.getFreezingDurationAsync();
  }

  public async submitAsync() {
    const finishDate = new Date(this.data.finishDate).getTime();
    const startDateWithFreezing = Date.now() + this.freezeTime * 24 * 3600 * 1000;

    if (this.data.totalTokens.isZero()) {
      this.notificationService.error(
        this.translateService.instant('StartAllotmentEventModalComponent.EmptyBalance')
      );
      this.dialogRef.close(false);
      return;
    }

    if (!this.data.finishDate || (finishDate > startDateWithFreezing) || finishDate < Date.now()) {
      this.notificationService.error(
        this.translateService.instant('StartAllotmentEventModalComponent.BadDate'),
      );
      this.dialogRef.close(false);
      return;
    }
    await this.allotmentEventService.startAsync(this.data.id);
    this.dialogRef.close(true);
  }
}
