import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms'
import { DeviceService, RawDevice } from '../deviceservice.service';

export interface Device {
  deviceId: string;
  status: 'Connected' | 'Disconnected';
  statusUpdateDate: Date;
  notes?: string;
  selected?: boolean;
  twin: any;
}

@Component({
  selector: 'app-device-list',
  imports: [FormsModule, CommonModule],
  templateUrl: './device-list.component.html',
  styleUrl: './device-list.component.css'
})
export class DeviceListComponent implements OnInit {
  devices: Device[] = [];
  filteredDevices: Device[] = [];

  statusFilter: '' | 'Connected' | 'Disconnected' = '';

  pageSize = 5;
  currentPage = 1;
  totalPages = 1;

  showModal = false;
  editingDevice: Device = { deviceId: '', status: 'Disconnected', statusUpdateDate: new Date(), notes: '', selected: false, twin: {} };
  editingTwinJson = '';
  isNew = false;

  constructor(private deviceService: DeviceService) {}

  ngOnInit() {
    this.deviceService.getDevices().subscribe((raw: RawDevice[]) => {
      this.devices = raw.map(d => ({
        deviceId: d.deviceId,
        status: d.connectionState === 1 ? 'Connected' : 'Disconnected',
        statusUpdateDate: new Date(d.lastActivityTime),
        notes: '',
        selected: false,
        twin: d.twin
      }));
      this.applyFilter();
    });
  }

  applyFilter() {
    this.filteredDevices = this.devices.filter(d => !this.statusFilter || d.status === this.statusFilter);
    this.currentPage = 1;
    this.updatePagination();
  }

  updatePagination() {
    this.totalPages = Math.ceil(this.filteredDevices.length / this.pageSize) || 1;
  }

  get pagedDevices(): Device[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredDevices.slice(start, start + this.pageSize);
  }

  prevPage() {
    if (this.currentPage > 1) this.currentPage--;
  }
  nextPage() {
    if (this.currentPage < this.totalPages) this.currentPage++;
  }

  get allSelected(): boolean {
    return this.pagedDevices.length > 0 && this.pagedDevices.every(d => d.selected);
  }
  get someSelected(): boolean {
    return this.pagedDevices.some(d => d.selected) && !this.allSelected;
  }
  toggleAll(event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    this.pagedDevices.forEach(d => d.selected = checked);
  }

  onAddDevice() {
    this.isNew = true;
    this.editingDevice = { deviceId: '', status: 'Disconnected', statusUpdateDate: new Date(), notes: '', selected: false, twin: {} };
    this.editingTwinJson = JSON.stringify(this.editingDevice.twin, null, 2);
    this.showModal = true;
  }

  openEditDialog(device: Device) {
    this.isNew = false;
    this.editingDevice = { ...device };
    this.editingTwinJson = JSON.stringify(this.editingDevice.twin, null, 2);
    this.showModal = true;
  }

  save() {
    try {
      this.editingDevice.twin = JSON.parse(this.editingTwinJson);
    } catch (e) {
      alert('Invalid JSON format for twin');
      return;
    }

    this.editingDevice.statusUpdateDate = new Date();
    if (this.isNew) {
      this.devices.push(this.editingDevice);
    } else {
      const idx = this.devices.findIndex(d => d.deviceId === this.editingDevice.deviceId);
      if (idx > -1) this.devices[idx] = this.editingDevice;
    }

    // opcionális backend frissítés
    // this.deviceService.updateDeviceTwin(this.editingDevice.deviceId, this.editingDevice.twin).subscribe();

    this.showModal = false;
    this.applyFilter();
  }

  closeModal() { this.showModal = false; }
}