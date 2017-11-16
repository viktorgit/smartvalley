import {Paths} from './paths';
import {RouterModule, Routes} from '@angular/router';
import {NgModule} from '@angular/core';
import {MetamaskHowtoComponent} from './components/metamask-howto/metamask-howto.component';
import {RootComponent} from './components/root/root.component';
import {ApplicationComponent} from './components/application/application.component';
import {IfAuthenticated} from './routing/IfAuthenticated';

const appRoutes: Routes = [
  {path: Paths.Root, pathMatch: 'full', component: RootComponent},
  {path: Paths.MetaMaskHowTo, pathMatch: 'full', component: MetamaskHowtoComponent},
  {path: Paths.Application, pathMatch: 'full', component: ApplicationComponent, canActivate: [IfAuthenticated]},
];

@NgModule({
  imports: [
    RouterModule.forRoot(appRoutes)
  ],
  exports: [
    RouterModule
  ]
})
export class AppRoutingModule {
}
