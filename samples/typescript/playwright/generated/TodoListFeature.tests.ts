// Generated code - do not modify
import { test, expect } from '@playwright/test';
import { TodoListFeatureBase } from './TodoListFeature.steps';

export abstract class TodoListFeatureTestsBase {
    protected abstract getSteps(page: Page): TodoListFeatureBase;

    test('Add Todo', async ({ page }) => {
        const steps = this.getSteps(page);

        await steps.whenINavigateTo('/todos');
        await steps.andIEnterInto('Buy groceries', 'new-todo');
        await steps.andIPressEnter();
        await steps.thenIShouldSeeTextInTheTodoList('Buy groceries');
    });

    test('Complete Todo', async ({ page }) => {
        const steps = this.getSteps(page);

        await steps.givenIHaveATodo('Buy groceries');
        await steps.whenINavigateTo('/todos');
        await steps.andIClickTheCheckboxNextTo('Buy groceries');
        await steps.thenTheTodoShouldBeMarkedAsCompleted('Buy groceries');
    });

}
