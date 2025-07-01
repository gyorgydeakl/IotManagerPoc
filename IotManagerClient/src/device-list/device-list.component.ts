import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms'
import { BatchJobDto, DeviceService, RawDevice } from '../deviceservice.service';
import { PickListModule } from 'primeng/picklist';
import 'jsoneditor/dist/jsoneditor.css';
import JSONEditor, { JSONEditorOptions } from 'jsoneditor';

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
  existingGroups: BatchJobDto[] = [];
  selectedBatchJobId = '';

  currentTag: Tag = { operation: 'create', key: '', value: '' };
  tags: Tag[] = [];

  currentPropertiesObj: any = {};
  private jsonEditor!: JSONEditor;
  @ViewChild('jsonEditorContainer', { static: false })
  private jsonEditorContainer!: ElementRef;

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
    }
  }

  executeCommand() {
    const selectedIds = this.devices.filter(d => d.selected).map(d => d.deviceId);
    if (!selectedIds.length) { alert('No devices selected'); return; }
    if (!this.commandText.trim()) { alert('Enter a command'); return; }
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
  continueGroup() {
    if (!this.groupName.trim()) { alert('Enter group name'); return; }
    this.groupStep = 1;
  }
  finishGroup(save: boolean): void {
    // Gather IDs and tags
    const deviceIds = this.selectedGroupDevices.map(d => d.deviceId);
    const tagsToSet = this.tags
      .filter(t => t.operation === 'create')
      .map(t => ({ key: t.key, value: t.value! }));
    const tagsToDelete = this.tags
      .filter(t => t.operation === 'delete')
      .map(t => t.key);

    // properties from editor
    const propsObj = this.jsonEditor ? this.jsonEditor.get() : {};
    const propertiesToSet = Object.entries(propsObj).map(
      ([key, value]) => ({ key, value })
    );
    const propertiesToDelete: string[] = [];

    if (save) {
      const createReq = {
        name: this.groupName,
        description: '',
        deviceIds,
        tagsToSet,
        tagsToDelete,
        propertiesToSet,
        propertiesToDelete
      };

      this.deviceService
        .createBatchJob(createReq)
        .subscribe({
          next: (job) => {
            this.deviceService
              .executeBatchJob(job.id)
              .subscribe({
                next: () => this.resetGroupModal(),
                error: (err) => alert('Exec failed: ' + err.message),
              });
          },
          error: (err) => alert('Create job failed: ' + err.message),
        });
    } else {
      const singleReq = { deviceIds, tagsToSet, tagsToDelete };
      this.deviceService
        .executeSingleTimeBatchJob(singleReq)
        .subscribe({
          next: () => this.resetGroupModal(),
          error: (err) => alert('Exec failed: ' + err.message),
        });
    }
  }

  private resetGroupModal(): void {
    this.showGroupModal = false;
    this.groupStep = 0;
    this.groupName = '';
    this.selectedGroupDevices = [];
    this.currentTag = { operation: 'create', key: '', value: '' };
    this.tags = [];
    this.messageInterval = '';
    this.currentPropertiesObj = {};
    if (this.jsonEditor) {
      this.jsonEditor.destroy();
      // null it so ngAfterViewChecked can re-init next time
      // @ts-ignore
      this.jsonEditor = undefined;
    }
  }

  closeGroupModal() { this.showGroupModal = false; }
  openLoadModal() {
    this.deviceService.getBatchJobsList()
      .subscribe(jobs => this.existingGroups = jobs);
    this.showLoadModal = true;
  }

  confirmLoad() {
    if (!this.selectedBatchJobId) { alert('Select a group'); return; }
    const job = this.existingGroups.find(j => j.id === this.selectedBatchJobId)!;
    // 1) Populate the group name
    this.groupName = job.name ?? '';

    // 2) Split your devices into “available” vs “selected”
    const ids = new Set(job.deviceIds || []);
    this.selectedGroupDevices = this.devices.filter(d => ids.has(d.deviceId));
    this.availableDevices = this.devices.filter(d => !ids.has(d.deviceId));

    // 3) Rebuild the Tag list
    this.tags = [];
    (job.tagsToSet || []).forEach(t =>
      this.tags.push({ operation: 'create', key: t.key, value: t.value })
    );
    (job.tagsToDelete || []).forEach(k =>
      this.tags.push({ operation: 'delete', key: k })
    );

    this.currentPropertiesObj = {};
    (job.propertiesToSet || []).forEach(p => {
      this.currentPropertiesObj[p.key] = p.value;
    });

    // 4) Close the load modal, show the main group modal on step 0
    this.showLoadModal = false;
    this.showGroupModal = true;
    this.groupStep = 0;
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
  removeTag(index: number): void {
    this.tags.splice(index, 1);
  }

  closeWizardModal() {
    this.resetGroupModal();
  }
  ngAfterViewChecked() {
    // only init once per step-change
    if (this.groupStep === 1 && this.jsonEditorContainer && !this.jsonEditor) {
      this.initJsonEditor();
    }
  }
  private initJsonEditor(): void {
    const options: JSONEditorOptions = {
      mode: 'tree',
      modes: ['code', 'tree'],   // allow user to switch
      mainMenuBar: true
    };
    this.jsonEditor = new JSONEditor(
      this.jsonEditorContainer.nativeElement,
      options
    );
    // seed with any pre-loaded properties (or empty object)
    this.jsonEditor.set(this.currentPropertiesObj);
  }
}