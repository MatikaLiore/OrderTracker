import { Routes } from '@angular/router';
import { OrderDetailComponent } from './orders/order-detail.component';
import { OrderFormComponent } from './orders/order-form.component';
import { OrderListComponent } from './orders/order-list.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'orders' },
  { path: 'orders', component: OrderListComponent },
  { path: 'orders/new', component: OrderFormComponent },
  { path: 'orders/:id', component: OrderDetailComponent },
  { path: '**', redirectTo: 'orders' }
];
