import { Page } from '@playwright/test';
import { LoginFeatureStepsBase } from '../generated/LoginFeature.steps';
import { LoginFeatureTestsBase } from '../generated/LoginFeature.tests';

class LoginFeatureSteps extends LoginFeatureStepsBase {
    constructor(private page: Page) {
        super();
    }

    async navigateTo(url: string): Promise<void> {
        await this.page.goto(url);
    }

    async enterIntoField(value: string, field: string): Promise<void> {
        await this.page.fill(`[data-testid="${field}"]`, value);
    }

    async clickButton(text: string): Promise<void> {
        await this.page.click(`button:has-text("${text}")`);
    }

    async verifyTextIsShown(text: string): Promise<void> {
        await this.page.waitForSelector(`text="${text}"`);
    }
}

export class LoginFeatureTests extends LoginFeatureTestsBase {
    protected getSteps(page: Page): LoginFeatureStepsBase {
        return new LoginFeatureSteps(page);
    }
} 