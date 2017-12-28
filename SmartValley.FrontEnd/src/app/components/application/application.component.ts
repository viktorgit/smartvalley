import {Component, ViewChild, ElementRef, ViewChildren, QueryList, OnInit} from '@angular/core';
import {AuthenticationService} from '../../services/authentication/authentication-service';
import {ApplicationApiClient} from '../../api/application/application-api.client';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {Application} from '../../services/application';
import {EnumTeamMemberType} from '../../services/enumTeamMemberType';
import {v4 as uuid} from 'uuid';
import {ProjectManagerContractClient} from '../../services/contract-clients/project-manager-contract-client';
import {Router} from '@angular/router';
import {Paths} from '../../paths';
import {NotificationsService} from 'angular2-notifications';
import {DialogService} from '../../services/dialog-service';
import {TranslateService} from '@ngx-translate/core';
import {BalanceService} from '../../services/balance/balance.service';
import {isNullOrUndefined} from 'util';

@Component({
  selector: 'app-application',
  templateUrl: './application.component.html',
  styleUrls: ['./application.component.css']
})
export class ApplicationComponent implements OnInit {
  public applicationForm: FormGroup;
  public isTeamShow = false;
  public isLegalShow = false;
  public isFinanceShow = false;
  public isTechShow = false;
  public isProjectCreating: boolean;

  @ViewChild('name') public nameRow: ElementRef;
  @ViewChild('area') public areaRow: ElementRef;
  @ViewChild('description') public descriptionRow: ElementRef;
  @ViewChildren('required') public requiredFields: QueryList<any>;

  constructor(private formBuilder: FormBuilder,
              private authenticationService: AuthenticationService,
              private projectManagerContractClient: ProjectManagerContractClient,
              private router: Router,
              private notificationsService: NotificationsService,
              private dialogService: DialogService,
              private applicationApiClient: ApplicationApiClient,
              private translateService: TranslateService,
              private balanceService: BalanceService) {
  }

  public ngOnInit(): void {
    this.createForm();
  }

  public showNext() {
    if (this.isTeamShow === false) {
      this.isTeamShow = true;
      return;
    }
    if (this.isLegalShow === false) {
      this.isLegalShow = true;
      return;
    }
    if (this.isFinanceShow === false) {
      this.isFinanceShow = true;
      return;
    }
    if (this.isTechShow === false) {
      this.isTechShow = true;
    }
  }

  public async onSubmitAsync() {
    if (!this.validateForm()) {
      return;
    }

    const projectCreationCost = await this.projectManagerContractClient.getProjectCreationCostAsync();
    if (!await this.dialogService.showSvtWithdrawalConfirmationDialogAsync(projectCreationCost)) {
      return;
    }

    await this.submitApplicationAsync();
  }

  private async submitApplicationAsync(): Promise<void> {
    const projectId = uuid();
    const transactionHash = await this.submitToContractAsync(projectId, this.applicationForm.value.name);
    if (transactionHash == null) {
      this.notificationsService.error(this.translateService.instant('Common.Error'), this.translateService.instant('Common.TryAgain'));
      this.isProjectCreating = false;
      return;
    }

    const transactionDialog = this.dialogService.showTransactionDialog(
      this.translateService.instant('Application.Dialog'),
      transactionHash
    );

    await this.submitToBackendAsync(projectId, transactionHash);
    await this.balanceService.updateBalanceAsync();

    transactionDialog.close();

    await this.router.navigate([Paths.Scoring], {queryParams: {tab: 'myProjects'}});
    this.notificationsService.success(
      this.translateService.instant('Common.Success'),
      this.translateService.instant('Application.ProjectCreated')
    );
  }

  private submitToBackendAsync(projectId: any, transactionHash: string): Promise<void> {
    const application = this.createApplication(projectId, transactionHash);
    return this.applicationApiClient.createApplicationAsync(application);
  }

  private createForm() {
    const teamMembers = [];
    for (const item in EnumTeamMemberType) {
      if (typeof EnumTeamMemberType[item] === 'number') {
        const group = this.formBuilder.group({
          memberType: EnumTeamMemberType[item],
          title: item,
          fullName: ['', Validators.maxLength(100)],
          facebookLink: ['', [Validators.maxLength(200), Validators.pattern('https?://.+')]],
          linkedInLink: ['', [Validators.maxLength(200), Validators.pattern('https?://.+')]],
        });
        teamMembers.push(group);
      }
    }

    this.applicationForm = this.formBuilder.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      whitePaperLink: ['', [Validators.maxLength(400), Validators.pattern('https?://.+')]],
      projectArea: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.maxLength(1000)]],
      projectStatus: ['', Validators.maxLength(100)],
      softCap: ['', Validators.maxLength(40)],
      hardCap: ['', Validators.maxLength(40)],
      financeModelLink: ['', [Validators.maxLength(400), Validators.pattern('https?://.+')]],
      country: ['', Validators.maxLength(100)],
      attractedInvestments: false,
      blockChainType: ['', Validators.maxLength(100)],
      mvpLink: ['', [Validators.maxLength(400), Validators.pattern('https?://.+')]],
      teamMembers: this.formBuilder.array(teamMembers)
    });
  }

  private createApplication(projectId: string, transactionHash: string): Application {
    const user = this.authenticationService.getCurrentUser();
    const form = this.applicationForm.value;
    return <Application>{
      attractedInvestments: form.attractedInvestments,
      blockChainType: form.blockChainType,
      country: form.country,
      financeModelLink: form.financeModelLink,
      hardCap: form.hardCap,
      mvpLink: form.mvpLink,
      name: form.name,
      description: form.description,
      projectArea: form.projectArea,
      projectStatus: form.projectStatus,
      softCap: form.softCap,
      whitePaperLink: form.whitePaperLink,
      projectId: projectId,
      authorAddress: user.account,
      teamMembers: form.teamMembers.filter(m => !isNullOrUndefined(m.fullName)),
      transactionHash: transactionHash
    };
  }

  public async submitToContractAsync(projectId: string, projectName: string): Promise<string> {
    try {
      return await this.projectManagerContractClient.addProjectAsync(projectId, projectName);
    } catch (e) {
      return null;
    }
  }

  private validateForm(): boolean {
    if (!this.applicationForm.invalid) {
      return true;
    }
    this.scrollToInvalidElement();
    return false;
  }

  private scrollToInvalidElement() {
    if (this.applicationForm.controls['name'].invalid) {
      this.scrollToElement(this.nameRow);
    } else if (this.applicationForm.controls['projectArea'].invalid) {
      this.scrollToElement(this.areaRow);
    } else if (this.applicationForm.controls['description'].invalid) {
      this.scrollToElement(this.descriptionRow);
    }
  }

  private scrollToElement(element: ElementRef) {
    const containerOffset = element.nativeElement.offsetTop;
    const fieldOffset = element.nativeElement.offsetParent.offsetTop;
    window.scrollTo({left: 0, top: containerOffset + fieldOffset - 15, behavior: 'smooth'});
    element.nativeElement.children[1].classList.add('ng-invalid');
    element.nativeElement.children[1].classList.add('ng-dirty');
    element.nativeElement.children[1].classList.remove('ng-valid');
    const invalidElements = this.requiredFields.filter(i => i.nativeElement.classList.contains('ng-invalid'));
    if (invalidElements.length > 0) {
      for (let a = 0; a < invalidElements.length; a++) {
        invalidElements[a].nativeElement.classList.add('ng-invalid');
        invalidElements[a].nativeElement.classList.add('ng-dirty');
      }
    }
  }
}
