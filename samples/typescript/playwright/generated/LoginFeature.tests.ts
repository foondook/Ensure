// Generated code - do not modify
import { test, expect, Page } from '@playwright/test';
import { LoginFeatureStepsBase } from './LoginFeature.steps';

export abstract class LoginFeatureTestsBase {
    protected abstract getSteps(page: Page): LoginFeatureStepsBase;

    test('Successful Login', async ({ page }) => {
        const steps = this.getSteps(page);

        await steps.navigateTo('/login');
        await steps.enterIntoField('test@example.com', 'email');
        await steps.enterIntoField('password123', 'password');
        await steps.clickButton('Sign In');
        await steps.verifyTextIsShown('Welcome back');
    });

    test('Invalid Credentials', async ({ page }) => {
        const steps = this.getSteps(page);

        await steps.navigateTo('/login');
        await steps.enterIntoField('wrong@example.com', 'email');
        await steps.enterIntoField('wrongpass', 'password');
        await steps.clickButton('Sign In');
        await steps.verifyTextIsShown('Invalid credentials');
    });

}
