import { VirtualMachine } from "./virtualMachine";

export class VirtualMachineRunner
{
    private _vm: VirtualMachine;
    public get vm() { return this._vm; }

    public running: boolean = false;
    public waitUntil: number = -1.0;
    public vmStepTiming: number = -1.0;

    public isWaiting: boolean = false;
    private interval: number | NodeJS.Timer = -1;
    private onComplete: (value: any) => void = () => {}
    private onError: (error: any) => void = () => {}

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
            try
            {
                this._vm.step();
            }
            catch (err)
            {
                this.running = false;
                this.onError(err);
            }
        }

        if (!this._vm.running)
        {
            this.running = false;
            this.onComplete(null);
            clearInterval(this.interval);
        }
    }

    public start(frameDt: number = 1000 / 60): Promise<any>
    {
        this.running = true;
        this._vm.running = true;
        this.interval = setInterval(this.step, frameDt, frameDt);

        return new Promise((resolve, reject) =>
        {
            this.onComplete = resolve;
            this.onError = reject;
        });
    }
}