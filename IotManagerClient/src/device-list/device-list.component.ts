import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms'
import { DeviceService, RawDevice } from '../deviceservice.service';
import { PickListModule } from 'primeng/picklist';

export interface Device {
  deviceId: string;
  status: 'Connected' | 'Disconnected';
  statusUpdateDate: Date;
  notes?: string;
  selected?: boolean;
  twin: any;
}
export interface DeviceGroup {
  name: string;
}
type TagOperation = 'create' | 'delete';
interface Tag {
  operation: TagOperation;
  key: string;
  value?: string;
}
@Component({
  selector: 'app-device-list',
  imports: [FormsModule, CommonModule, PickListModule],
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

  showEditModal = false;
  editingDevice: Device = { deviceId: '', status: 'Disconnected', statusUpdateDate: new Date(), notes: '', selected: false, twin: {} };
  editingTwinJson = '';
  isNew = false;

  showCommandModal = false;
  commandText = '';
  showGroupModal = false;
  groupStep = 0;
  groupName = '';
  availableDevices: Device[] = [];
  selectedGroupDevices: Device[] = [];
  messageInterval = '';

  showLoadModal = false;
  existingGroups: DeviceGroup[] = [];
  selectedGroupName = '';

  currentTag: Tag = { operation: 'create', key: '', value: '' };
  tags: Tag[] = [];

  constructor(private deviceService: DeviceService) { }

  ngOnInit() {
    this.loadDevices();
  }

  loadDevices() {
    this.deviceService.getDevices().subscribe((raw: RawDevice[]) => {
      this.devices = raw.map(d => ({
        deviceId: d.deviceId,
        status: d.connectionState === 1 ? 'Connected' : 'Disconnected',
        statusUpdateDate: new Date(d.lastActivityTime),
        notes: '', selected: false, twin: d.twin
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
    return this.filteredDevices.slice((this.currentPage - 1) * this.pageSize, this.currentPage * this.pageSize);
  }

  prevPage() { if (this.currentPage > 1) this.currentPage--; }
  nextPage() { if (this.currentPage < this.totalPages) this.currentPage++; }

  // Selection
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
    this.showEditModal = true;
  }

  openEditDialog(device: Device) {
    this.isNew = false;
    this.editingDevice = { ...device };
    this.editingTwinJson = JSON.stringify(this.editingDevice.twin, null, 2);
    this.showEditModal = true;
  }

  save() {
    if (this.isNew) {
      this.deviceService.createDevice(this.editingDevice.deviceId).subscribe(newRaw => {
        const newDev: Device = {
          deviceId: newRaw.deviceId,
          status: newRaw.connectionState === 1 ? 'Connected' : 'Disconnected',
          statusUpdateDate: new Date(newRaw.lastActivityTime),
          notes: '',
          selected: false,
          twin: newRaw.twin
        };
        this.devices.push(newDev);
        this.applyFilter();
        this.showEditModal = false;
      }, err => alert('Error creating device'));
    } else {
      try {
        this.editingDevice.twin = JSON.parse(this.editingTwinJson);
      } catch {
        alert('Invalid JSON format for twin');
        return;
      }
      this.deviceService.updateDeviceTwin(this.editingDevice.deviceId, this.editingDevice.twin).subscribe(() => {
        this.editingDevice.statusUpdateDate = new Date();
        const idx = this.devices.findIndex(d => d.deviceId === this.editingDevice.deviceId);
        if (idx > -1) this.devices[idx] = { ...this.editingDevice };
        this.applyFilter();
        this.showEditModal = false;
      }, err => alert('Error updating twin'));
    }
  }

  executeCommand() {
    const selectedIds = this.devices.filter(d => d.selected).map(d => d.deviceId);
    if (!selectedIds.length) { alert('No devices selected'); return; }
    if (!this.commandText.trim()) { alert('Enter a command'); return; }
    this.deviceService.executeCommand(selectedIds, this.commandText).subscribe();
    this.showCommandModal = false;
    this.commandText = '';
  }

  closeModals() {
    this.showEditModal = false;
    this.showCommandModal = false;
  }
  openGroupModal() {
    this.groupStep = 0;
    this.groupName = '';
    this.availableDevices = this.devices.filter(d => !d.selected);
    this.selectedGroupDevices = [];
    this.showGroupModal = true;
  }
  continueGroup(save: boolean) {
    if (save) {
      if (!this.groupName.trim()) { alert('Enter group name'); return; }
    }
    this.groupStep = 1;
  }
  finishGroup() {
    this.showGroupModal = false;
    this.groupStep = 0;
    this.groupName = '';
    this.selectedGroupDevices = [];
    this.currentTag = { operation: 'create', key: '', value: '' };
    this.tags = [];
  }
  closeGroupModal() { this.showGroupModal = false; }
  openLoadModal() {
    this.showLoadModal = true;
    this.deviceService.getDeviceGroups().subscribe(groups => {
      this.existingGroups = groups;
    });
  }
  confirmLoad() {
    if (!this.selectedGroupName) { alert('Select a group'); return; }
    this.showLoadModal = false;
    this.openGroupModal();
    this.groupName = this.selectedGroupName;
    // TODO: fetch and set selectedGroupDevices
  }
  addTag(): void {
    const op = this.currentTag.operation;
    if (!this.currentTag.key || (op === 'create' && !this.currentTag.value)) {
      return;
    }
    this.tags.push({
      operation: op,
      key: this.currentTag.key,
      value: op === 'create' ? this.currentTag.value : undefined
    });
    this.currentTag = { operation: 'create', key: '', value: '' };
  }
  updateOperation(value: string): void {
    this.currentTag.operation = value as TagOperation;
  }
}