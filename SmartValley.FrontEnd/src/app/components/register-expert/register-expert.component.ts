import {Component, ElementRef, OnInit, QueryList, ViewChildren} from '@angular/core';
import {Paths} from '../../paths';
import {Router} from '@angular/router';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {ExpertApiClient} from '../../api/expert/expert-api-client';
import {CreateExpertApplicationRequest} from '../../api/expert/expert-applitacion-request';
import {ExpertsRegistryContractClient} from '../../services/contract-clients/experts-registry-contract-client';
import {AreaType} from '../../api/scoring/area-type.enum';
import {DialogService} from '../../services/dialog-service';
import {SexEnum} from './sex.enum';
import {DocumentEnum} from './document.enum';
import {Country} from '../../services/common/country';
import {TranslateService} from '@ngx-translate/core';
import {SelectItem} from 'primeng/api';
import {UserContext} from '../../services/authentication/user-context';
import * as moment from 'moment';
import {NotificationsService} from 'angular2-notifications';
import {AreaService} from '../../services/expert/area.service';
import {EnumHelper} from '../../utils/enum-helper';
import {isUndefined} from 'util';
import {Md5} from 'ts-md5';

const countries = <Country[]>require('../../../assets/countryList.json');

@Component({
  selector: 'app-register-expert',
  templateUrl: './register-expert.component.html',
  styleUrls: ['./register-expert.component.css']
})
export class RegisterExpertComponent implements OnInit {

  public registryForm: FormGroup;
  public isLetterShow = false;
  public isKYCShow = false;
  public isProjectCreating: boolean;

  public sex: SelectItem[];
  public documentTypes: SelectItem[];
  public countries: SelectItem[];
  public areas: SelectItem[];
  public selectedAreas: AreaType[] = [];

  @ViewChildren('required') public requiredFields: QueryList<any>;

  private cv: File;
  private document: File;
  private photo: File;

  constructor(private formBuilder: FormBuilder,
              private router: Router,
              private expertApiClient: ExpertApiClient,
              private dialogService: DialogService,
              private userContext: UserContext,
              private notificationsService: NotificationsService,
              private translateService: TranslateService,
              private expertsRegistryContractClient: ExpertsRegistryContractClient,
              private areaService: AreaService,
              private enumHelper: EnumHelper) {
  }

  public ngOnInit(): void {
    this.createForm();
  }

  public showNext() {
    if (this.isLetterShow === false) {
      this.isLetterShow = true;
      return;
    }
    if (this.isKYCShow === false) {
      this.isKYCShow = true;
      return;
    }
  }

  private createForm() {
    this.sex = this.enumHelper.getSexes();
    this.documentTypes = this.enumHelper.getDocumentTypes();

    this.areas = this.areaService.areas.map(a => <SelectItem> {
      label: a.name,
      value: +a.areaType
    });

    this.countries = [];

    for (const item of countries) {
      this.countries.push({label: item.name, value: item});
    }

    this.registryForm = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      secondName: ['', [Validators.required, Validators.maxLength(50)]],
      linkedin: ['', [Validators.required, Validators.maxLength(400), Validators.pattern('https?://.+')]],
      facebook: ['', [Validators.required, Validators.maxLength(400), Validators.pattern('https?://.+')]],
      why: ['', [Validators.required, Validators.maxLength(1500)]],
      description: ['', [Validators.required, Validators.maxLength(1500)]],
      country: [this.countries[0].value],
      city: ['', [Validators.required, Validators.maxLength(50)]],
      selectedSex: [SexEnum.Male],
      selectedDocumentType: [DocumentEnum.Passport],
      birthDate: ['', [Validators.required, Validators.maxLength(100)]],
      number: ['', [Validators.required, Validators.maxLength(30)]]
    });
  }

  public async applyAsync(): Promise<void> {
    if (!this.validateForm()) {
      return;
    }

    const isSucceeded = await this.submitAsync();
    if (isSucceeded) {
      await this.router.navigate([Paths.Root]);
    }
  }

  public onCvUpload(event: any) {
    this.cv = event.files[0];
  }

  public onDocumentUpload(event: any) {
    this.document = event.files[0];
  }

  public onPhotoUpload(event: any) {
    this.photo = event.files[0];
  }

  private setInvalid(element: ElementRef) {
    element.nativeElement.classList.add('ng-invalid');
    element.nativeElement.classList.add('ng-dirty');
  }

  private validateForm(): boolean {
    if (!this.registryForm.invalid) {
      return true;
    }

    const invalidElements = this.requiredFields.filter(
      i => i.el !== undefined ?
        i.el.nativeElement.classList.contains('ng-invalid')
        : i.nativeElement.classList.contains('ng-invalid'));

    if (invalidElements.length > 0) {
      for (let a = 0; a < invalidElements.length; a++) {
        const element = invalidElements[a].el !== undefined ? invalidElements[a].el : invalidElements[a];
        this.setInvalid(element);
      }
      const firstElement = invalidElements[0].el !== undefined ? invalidElements[0].el : invalidElements[0];
      this.scrollToElement(firstElement);
    }
    return false;
  }

  private scrollToElement(element: ElementRef) {
    const offsetTop1 = element.nativeElement.offsetTop;
    const offsetTop3 = element.nativeElement.offsetParent.offsetParent.offsetTop;
    window.scrollTo({left: 0, top: offsetTop1 + offsetTop3, behavior: 'smooth'});
  }

  private createExpertApplicationRequest(transactionHash: string, areas: Array<AreaType>): CreateExpertApplicationRequest {
    const user = this.userContext.getCurrentUser();
    const form = this.registryForm.value;
    const input = new FormData();
    input.append('scan', this.document);
    input.append('photo', this.photo);
    input.append('cv', this.cv);
    input.append('transactionHash', transactionHash);
    input.append('sex', (<number>form.selectedSex).toString());
    input.append('applicantAddress', user.account);
    input.append('birthDate', moment(form.birthDate).toISOString());
    input.append('city', form.city);
    input.append('countryIsoCode', form.country.code);
    input.append('documentNumber', form.number);
    input.append('documentType', form.selectedDocumentType);
    input.append('facebookLink', form.facebook);
    input.append('linkedInLink', form.linkedIn);
    input.append('firstName', form.firstName);
    input.append('lastName', form.secondName);
    input.append('description', form.description);
    input.append('why', form.why);
    areas.forEach(a => input.append('areas', a.toString()));

    return <CreateExpertApplicationRequest>{
      body: input
    };
  }

  private async submitAsync(): Promise<boolean> {
    const areas = this.selectedAreas.map(a => +a);
    if (areas.length === 0) {
      this.notificationsService.error(this.translateService.instant('RegisterExpert.CategoryNotSelectedError'));
      return false;
    }
    if (isUndefined(this.document)) {
      this.notificationsService.error(this.translateService.instant('RegisterExpert.DocumentNotUploadedError'));
      return false;
    }
    if (isUndefined(this.photo)) {
      this.notificationsService.error(this.translateService.instant('RegisterExpert.PhotoNotUploadedError'));
      return false;
    }
    if (isUndefined(this.cv)) {
      this.notificationsService.error(this.translateService.instant('RegisterExpert.CVNotUploadedError'));
      return false;
    }

    const applicationHash = this.getApplicationHash(areas);

    const transactionHash = await this.applyToContractAsync(areas, applicationHash);
    if (transactionHash == null) {
      return false;
    }

    const transactionDialog = this.dialogService.showTransactionDialog(
      this.translateService.instant('RegisterExpert.Dialog'),
      transactionHash
    );

    const request = this.createExpertApplicationRequest(transactionHash, areas);

    await this.expertApiClient.createApplicationAsync(request);

    transactionDialog.close();
    return true;
  }

  private async applyToContractAsync(areas: Array<AreaType>, applicationHash: string): Promise<string> {
    try {
      return await this.expertsRegistryContractClient.applyAsync(areas, applicationHash);
    } catch (e) {
      return null;
    }
  }

  private getApplicationHash(areas: Array<AreaType>): string {
    const form = this.registryForm.value;
    const applicationStr = (<number>form.selectedSex).toString() +
      moment(form.birthDate).toISOString() +
      form.city +
      form.country.code +
      form.number +
      form.selectedDocumentType +
      form.facebook +
      form.linkedIn +
      form.firstName +
      form.secondName +
      form.description +
      form.why;

    areas.forEach(a => applicationStr.concat(a.toString()));
    return '0x' + Md5.hashStr(applicationStr, false).toString();
  }
}
