// Generated code - do not modify
import { LoginFeatureBase } from './LoginFeature.steps';

export abstract class LoginFeatureTestsBase {
    protected abstract getSteps(): LoginFeatureBase;

    async successfulLogin() {
        const steps = this.getSteps();

        await steps.navigateTo('/login');
        await steps.enterIntoField('test@example.com', 'email');
        await steps.enterIntoField('password123', 'password');
        await steps.clickButton('Sign In');
        await steps.verifyTextIsShown('Welcome back');
    }

    async invalidCredentials() {
        const steps = this.getSteps();

        await steps.navigateTo('/login');
        await steps.enterIntoField('wrong@example.com', 'email');
        await steps.enterIntoField('wrongpass', 'password');
        await steps.clickButton('Sign In');
        await steps.verifyTextIsShown('Invalid credentials');
    }

}
