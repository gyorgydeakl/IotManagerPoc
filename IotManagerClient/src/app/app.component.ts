import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DeviceListComponent } from '../device-list/device-list.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, DeviceListComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'IotManagerClient';
}
