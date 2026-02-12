import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useNavigate, Link } from 'react-router-dom';
import api from '../api/api';
import { useState } from 'react';

const onboardSchema = z.object({
  username: z.string().min(1, 'Username is required'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
});

type OnboardForm = z.infer<typeof onboardSchema>;

export default function OnboardPage() {
  const { register, handleSubmit, formState: { errors } } = useForm<OnboardForm>({
    resolver: zodResolver(onboardSchema),
  });
  const navigate = useNavigate();
  const [error, setError] = useState('');

  const onSubmit = async (data: OnboardForm) => {
    try {
      await api.post('/user/onboard', data);
      // Auto login after onboard? Backend doesn't return cookie on onboard, so we need to login manually or prompt user.
      // Let's redirect to login.
      navigate('/login');
    } catch (err: any) {
      setError('Registration failed: ' + (err.response?.data?.message || err.message || JSON.stringify(err)));
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <div className="w-full max-w-md p-8 space-y-6 bg-white rounded shadow-md">
        <h2 className="text-2xl font-bold text-center">Register</h2>
        {error && <div className="p-2 text-red-500 bg-red-100 rounded">{error}</div>}
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block mb-1 text-sm font-medium">Username</label>
            <input
              {...register('username')}
              className="w-full px-3 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.username && <span className="text-xs text-red-500">{errors.username.message}</span>}
          </div>
          <div>
            <label className="block mb-1 text-sm font-medium">Password</label>
            <input
              type="password"
              {...register('password')}
              className="w-full px-3 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.password && <span className="text-xs text-red-500">{errors.password.message}</span>}
          </div>
          <button
            type="submit"
            className="w-full px-4 py-2 text-white bg-green-600 rounded hover:bg-green-700"
          >
            Register
          </button>
        </form>
        <p className="text-sm text-center">
          Already have an account? <Link to="/login" className="text-blue-600 hover:underline">Login</Link>
        </p>
      </div>
    </div>
  );
}
