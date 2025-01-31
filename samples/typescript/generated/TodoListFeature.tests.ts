// Generated code - do not modify
import { TodoListFeatureBase } from './TodoListFeature.steps';

export abstract class TodoListFeatureTestsBase {
    protected abstract getSteps(): TodoListFeatureBase;

    async addTodo() {
        const steps = this.getSteps();

        await steps.whenINavigateTo('/todos');
        await steps.andIEnterInto('Buy groceries', 'new-todo');
        await steps.andIPressEnter();
        await steps.thenIShouldSeeTextInTheTodoList('Buy groceries');
    }

    async completeTodo() {
        const steps = this.getSteps();

        await steps.givenIHaveATodo('Buy groceries');
        await steps.whenINavigateTo('/todos');
        await steps.andIClickTheCheckboxNextTo('Buy groceries');
        await steps.thenTheTodoShouldBeMarkedAsCompleted('Buy groceries');
    }

}
