import { VirtualMachine } from "./virtualMachine";

export class VirtualMachineRunner
{
    private _vm: VirtualMachine;
    public get vm() { return this._vm; }

    public running: boolean = false;
    public waitUntil: number = -1.0;
    public vmStepTiming: number = -1.0;

    public isWaiting: boolean = false;
    private interval: number = -1;
    private onComplete: (value: any) => void = () => {}

    constructor (vm: VirtualMachine)
    {
        this._vm = vm;
    }

    public step = (dt: number) =>
    {
        if (!this.running)
        {
            return;
        }

        let runOnce = false;
        while (this._vm.running && !this._vm.paused)
        {
            if (this.waitUntil > 0.0)
            {
                this.waitUntil -= dt;
                if (runOnce || this.waitUntil > 0.0)
                {
                    break;
                }
                else
                {
                    this.waitUntil = this.vmStepTiming;
                }
            }
            else
            {
                this.waitUntil = this.vmStepTiming;
            }

            runOnce = true;
            this.isWaiting = false;
            this._vm.step();
        }

        if (!this._vm.running)
        {
            this.running = false;
            this.onComplete(null);
            clearInterval(this.interval);
        }
    }

    public start(): Promise<any>
    {
        this.running = true;
        this._vm.running = true;
        this.interval = setInterval(this.step, 1);

        return new Promise((resolve, reject) =>
        {
            this.onComplete = resolve;
        });
    }
}