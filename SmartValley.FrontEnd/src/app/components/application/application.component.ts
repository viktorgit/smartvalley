import {Component} from '@angular/core';
import {AuthenticationService} from '../../services/authentication-service';
import {ApplicationApiClient} from '../../api/application/application-api.client';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {Application} from '../../services/application';
import {EnumTeamMemberType} from '../../services/enumTeamMemberType';
import {v4 as uuid} from 'uuid';
import {ProjectManagerContractClient} from '../../services/project-manager-contract-client';
import {ContractApiClient} from '../../api/contract/contract-api-client';
import {Router} from '@angular/router';
import {Paths} from '../../paths';

@Component({
  selector: 'app-application',
  templateUrl: './application.component.html',
  styleUrls: ['./application.component.css']
})
export class ApplicationComponent {

  applicationForm: FormGroup;
  isTeamShow = false;
  isLegalShow = false;
  isFinanceShow = false;
  isTechShow = false;

  constructor(private formBuilder: FormBuilder,
              private authenticationService: AuthenticationService,
              private applicationApiClient: ApplicationApiClient,
              private contractApiClient: ContractApiClient,
              private projectManagerContractClient: ProjectManagerContractClient,
              private router: Router) {
    this.createForm();
  }

  createForm() {

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

  showNext() {
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
      return;
    }
  }

  async onSubmit() {
    const application = await this.fillApplication();
    await this.applicationApiClient.createApplicationAsync(application);
    await this.router.navigate([Paths.Scoring], {queryParams: {tab: 'myProjects'}});
  }

  private async fillApplication(): Promise<Application> {

    const formModel = this.applicationForm.value;

    const application = {} as Application;
    console.log(formModel.attractedInvestments);
    application.attractedInvestments = formModel.attractedInvestments;
    application.blockChainType = formModel.blockChainType;
    application.country = formModel.country;
    application.financeModelLink = formModel.financeModelLink;
    application.hardCap = formModel.hardCap;
    application.mvpLink = formModel.mvpLink;
    application.name = formModel.name;
    application.description = formModel.description;
    application.projectArea = formModel.projectArea;
    application.projectStatus = formModel.projectStatus;
    application.softCap = formModel.softCap;
    application.whitePaperLink = formModel.whitePaperLink;

    application.projectId = uuid();

    const user = await this.authenticationService.getCurrentUser();
    application.authorAddress = user.account;
    application.teamMembers = [];
    for (const teamMember of formModel.teamMembers) {
      if (teamMember.fullName) {
        application.teamMembers.push(teamMember);
      }
    }

    const projectManagerContract = await this.contractApiClient.getProjectManagerContractAsync();
    application.transactionHash = await this.projectManagerContractClient.addProjectAsync(
      projectManagerContract.address,
      projectManagerContract.abi,
      application.projectId,
      formModel.name);

    return application;
  }
}
